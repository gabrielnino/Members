using System.Diagnostics;
using Configuration;
using LiveNetwork.Application.Services;
using LiveNetwork.Domain;
using Microsoft.Extensions.Logging;
using Services.Interfaces;

namespace LiveNetwork.Infrastructure.Services
{
    public class PromptGenerator(IOpenAIClient openAIClient,
                                 ILogger<PromptGenerator> logger,
                                 ITrackingService trackingService,
                                 AppConfig config) : IPromptGenerator
    {
        private readonly IOpenAIClient _openAIClient = openAIClient ?? throw new ArgumentNullException(nameof(openAIClient));
        private readonly ILogger<PromptGenerator> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly ITrackingService _trackingService = trackingService ?? throw new ArgumentNullException(nameof(trackingService));
        private readonly AppConfig _config = config ?? throw new ArgumentNullException(nameof(config));

        private static readonly Random _random = new();
        private int MaxInvites = 100; // Limit to 10 invites per run

        /// <summary>
        /// Generates up to MaxInvites first-time invites, with detailed step-by-step logging.
        /// </summary>
        public async Task GeneratPrompt() // kept for compatibility
        {
            var runId = Guid.NewGuid().ToString("N");
            var swRun = Stopwatch.StartNew();

            using (_logger.BeginScope(new Dictionary<string, object?>
            {
                ["RunId"] = runId
            }))
            {
                _logger.LogInformation("Starting invite generation run. MaxInvites={MaxInvites}.", MaxInvites);

                try
                {
                    MaxInvites = Math.Max(1, Math.Min(1000, _config.Options.MaxInvites));
                    // 1) Load profiles
                    var profilesPath = _config.Paths.DetailedProfilesOutputFilePath;
                    _logger.LogInformation("Step 1/6: Loading detailed profiles from path: {ProfilesPath}", profilesPath);

                    var swProfiles = Stopwatch.StartNew();
                    var profiles = await _trackingService.LoadDetailedProfilesAsync(profilesPath);
                    swProfiles.Stop();

                    if (profiles == null || profiles.Count == 0)
                    {
                        _logger.LogInformation("No profiles found (elapsed {ElapsedMs} ms). Nothing to do.", swProfiles.ElapsedMilliseconds);
                        return;
                    }
                    _logger.LogInformation("Loaded {ProfileCount} profiles (elapsed {ElapsedMs} ms).",
                        profiles.Count, swProfiles.ElapsedMilliseconds);

                    // 2) Load or initialize conversation threads
                    var threadsPath = _config.Paths.ConversationOutputFilePath;
                    _logger.LogInformation("Step 2/6: Loading conversation threads from path: {ThreadsPath}", threadsPath);

                    var swThreads = Stopwatch.StartNew();
                    var threads = await _trackingService.LoadConversationThreadAsync(threadsPath) ?? [];
                    swThreads.Stop();

                    if (threads.Count == 0)
                    {
                        _logger.LogInformation("No existing threads found. Initializing from profiles...");
                        var swInit = Stopwatch.StartNew();
                        threads.AddRange(profiles.Select(p => new ConversationThread(p)));
                        swInit.Stop();
                        _logger.LogInformation("Initialized {ThreadCount} threads from profiles (elapsed {ElapsedMs} ms).",
                        threads.Count, swInit.ElapsedMilliseconds);
                    }
                    else
                    {
                        _logger.LogInformation("Loaded {ThreadCount} existing threads (elapsed {ElapsedMs} ms).",
                            threads.Count, swThreads.ElapsedMilliseconds);
                    }

                    // 3) Determine eligible threads (no initial invite yet)
                    _logger.LogInformation("Step 3/6: Selecting eligible threads (no initial invite).");
                    var eligible = threads.Where(t => !t.HasActivity).ToList();
                    _logger.LogInformation("{EligibleCount} threads are eligible out of {TotalThreads}.",
                        eligible.Count, threads.Count);

                    if (eligible.Count == 0)
                    {
                        _logger.LogInformation("All threads already have an initial invite. Exiting.");
                        return;
                    }

                    // 4) Shuffle and select up to MaxInvites
                    _logger.LogInformation("Step 4/6: Shuffling eligible threads and taking up to {MaxInvites}.", MaxInvites);
                    var swShuffle = Stopwatch.StartNew();
                    ShuffleInPlace(eligible);
                    var selected = eligible.Take(MaxInvites).ToList();
                    swShuffle.Stop();

                    _logger.LogInformation("Selected {SelectedCount} threads (elapsed {ElapsedMs} ms).",
                        selected.Count, swShuffle.ElapsedMilliseconds);

                    // 5) Build prompt, call OpenAI, and add invite for each selected thread
                    _logger.LogInformation("Step 5/6: Generating invites for selected threads.");
                    int successCount = 0;
                    int failCount = 0;

                    for (int i = 0; i < selected.Count; i++)
                    {
                        var thread = selected[i];
                        using (_logger.BeginScope(new Dictionary<string, object?>
                        {
                            ["ThreadIndex"] = i,
                            ["SelectedCount"] = selected.Count,
                            ["RunId"] = runId
                        }))
                        {
                            _logger.LogDebug("Processing thread {Index}/{Total}.", i + 1, selected.Count);

                            try
                            {
                                _logger.LogDebug("Building prompt for target profile.");
                                var swPrompt = Stopwatch.StartNew();
                                var prompt = InvitePrompt.BuildPrompt(thread.TargetProfile);
                                swPrompt.Stop();
                                _logger.LogDebug("Prompt built (elapsed {ElapsedMs} ms).", swPrompt.ElapsedMilliseconds);

                                _logger.LogDebug("Requesting OpenAI chat completion...");
                                var swAi = Stopwatch.StartNew();
                                var content = "Hi, I’m always curious about the challenges others are working through. I’ve learned so much from colleagues and peers, and I’d love to connect—maybe we can share ideas and support each other along the way.";

                                if (_config.Options.EnableCustomMessages)
                                {
                                    content = await _openAIClient.GetChatCompletionAsync(prompt);
                                }
                                swAi.Stop();
                                _logger.LogDebug("Received OpenAI response (elapsed {ElapsedMs} ms, ContentLength={ContentLength}).",
                                    swAi.ElapsedMilliseconds, content?.Length ?? 0);

                                _logger.LogDebug("Adding invite to thread.");
                                if (string.IsNullOrWhiteSpace(content))
                                    throw new InvalidOperationException("Received empty content from OpenAI.");
                                var invite = new Invite(content, InvitePrompt.Experiment, InviteStatus.Draft);
                                var contentScore = string.Empty;
                                if (_config.Options.EnableCustomMessages)
                                {
                                    var scorePrompt = ScorePrompt.BuildSandlerReviewPrompt(content, thread.TargetProfile);
                                    contentScore = await _openAIClient.GetChatCompletionAsync(scorePrompt);
                                }
                                invite.FeedbackNotes = contentScore;
                                thread.AddInvite(invite);

                                successCount++;
                                _logger.LogInformation("Invite added successfully for thread {Index}.", i + 1);

                                _logger.LogInformation("Step 6/6: Saving conversation threads to path: {ThreadsPath}", threadsPath);
                                await _trackingService.SaveConversationThreadAsync(threads, threadsPath);
                            }
                            catch (Exception exThread)
                            {
                                failCount++;
                                _logger.LogError(exThread, "Failed to generate invite for thread {Index}. Continuing with next.", i + 1);
                            }
                        }
                    }

                    _logger.LogInformation("Invite generation complete: Success={SuccessCount}, Failed={FailCount}.", successCount, failCount);

                    // 6) Persist conversation threads
                    _logger.LogInformation("Step 6/6: Saving conversation threads to path: {ThreadsPath}", threadsPath);
                    var swSave = Stopwatch.StartNew();
                    await _trackingService.SaveConversationThreadAsync(threads, threadsPath);
                    swSave.Stop();
                    _logger.LogInformation("Conversation threads saved (elapsed {ElapsedMs} ms).", swSave.ElapsedMilliseconds);

                    _logger.LogInformation("Run finished successfully in {ElapsedMs} ms. RunId={RunId}.", swRun.ElapsedMilliseconds, runId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Run failed with an unhandled error. RunId={RunId}.", runId);
                    throw;
                }
                finally
                {
                    swRun.Stop();
                }
            }
        }

        private static void ShuffleInPlace<T>(IList<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = _random.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
    }
}
