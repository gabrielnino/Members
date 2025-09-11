using System.Text;
using System.Text.Json;
using Configuration;
using LiveNetwork.Application.Services;
using LiveNetwork.Domain;
using Microsoft.Extensions.Logging;


namespace LiveNetwork.Infrastructure.Services
{
    public class TrackingService : ITrackingService
    {
        private readonly ILogger<TrackingService> _logger;
        private readonly ExecutionTracker _executionOptions;
        private readonly SemaphoreSlim _fileLock = new(1, 1);
        private readonly JsonSerializerOptions _jsonOptions;

        public TrackingService(ILogger<TrackingService> logger, ExecutionTracker executionOptions)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _executionOptions = executionOptions ?? throw new ArgumentNullException(nameof(executionOptions));

            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNameCaseInsensitive = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
        }


        public async Task<TrackingState> LoadStateAsync()
        {

            await _fileLock.WaitAsync();
            try
            {
                //var searchId = GenerateSearchId(searchText);
                var filePath = Path.Combine(_executionOptions.ExecutionFolder, $"page_tracking.json");

                if (!File.Exists(filePath))
                    return new TrackingState();

                await using var fileStream = File.OpenRead(filePath);
                return await JsonSerializer.DeserializeAsync<TrackingState>(fileStream, _jsonOptions)
                    ?? new TrackingState();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading page state for search");
                throw;
            }
            finally
            {
                _fileLock.Release();
            }
        }

        public async Task<List<Uri>> LoadConnectionsFromSearchAsync(string searchUrlOutputFilePath)
        {
            try
            {
                var filePath = Path.Combine(_executionOptions.ExecutionFolder, searchUrlOutputFilePath);
                if (!File.Exists(filePath))
                {
                    return [];
                }
                await using var fileStream = File.OpenRead(filePath);
                var connections = await JsonSerializer.DeserializeAsync<List<Uri>>(fileStream, _jsonOptions);
                return connections ?? [];
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading search Url output filepath: {SearchText}", searchUrlOutputFilePath);
                throw;
            }
            finally
            {
            }
        }

        public async Task SavePageStateAsync(TrackingState state)
        {

            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            await _fileLock.WaitAsync();
            try
            {
                var filePath = Path.Combine(_executionOptions.ExecutionFolder, $"page_tracking.json");
                await using var fileStream = File.Create(filePath);
                await JsonSerializer.SerializeAsync(fileStream, state, _jsonOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving page state for search");
                throw;
            }
            finally
            {
                _fileLock.Release();
            }
        }

        public async Task SaveConnectionsAsync(List<Uri> connections, string searchUrlOutputFilePath)
        {

            if (connections == null)
            {
                throw new ArgumentNullException(nameof(connections));
            }

            await _fileLock.WaitAsync();
            try
            {
                var filePath = Path.Combine(_executionOptions.ExecutionFolder, searchUrlOutputFilePath);
                await using var fileStream = File.Create(filePath);
                await JsonSerializer.SerializeAsync(fileStream, connections, _jsonOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving page state for search");
                throw;
            }
            finally
            {
                _fileLock.Release();
            }
        }

        public async Task<List<ConnectionInfo>> LoadCollectorConnectionsAsync(string collectorInfoOutputFilePath)
        {
            try
            {
                var filePath = Path.Combine(_executionOptions.ExecutionFolder, collectorInfoOutputFilePath);
                if (!File.Exists(filePath))
                {
                    return [];
                }
                await using var fileStream = File.OpenRead(filePath);
                var connections = await JsonSerializer.DeserializeAsync<List<ConnectionInfo>>(fileStream, _jsonOptions);
                return connections ?? [];
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading search Url output filepath: {SearchText}", collectorInfoOutputFilePath);
                throw;
            }
            finally
            {
            }
        }


        public async Task<DateTime> LoadLastProcessedDateUtcAsync(string deltaFileName)
        {
            var filePath = Path.Combine(_executionOptions.ExecutionFolder, deltaFileName);
            try
            {
                if (!File.Exists(filePath)) return DateTime.MinValue;
                var text = await File.ReadAllTextAsync(filePath);
                if (DateTime.TryParse(text, null, System.Globalization.DateTimeStyles.AdjustToUniversal | System.Globalization.DateTimeStyles.AssumeUniversal, out var dt))
                    return DateTime.SpecifyKind(dt, DateTimeKind.Utc);
                return DateTime.MinValue;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not read last processed date at {Path}. Starting from MinValue.", filePath);
                return DateTime.MinValue;
            }
        }

        public async Task SaveLastProcessedDateUtcAsync(string deltaFileName, DateTime dtUtc)
        {
            var filePath = Path.Combine(_executionOptions.ExecutionFolder, deltaFileName);
            try
            {
                var text = dtUtc.ToUniversalTime().ToString("O");
                await File.WriteAllTextAsync(filePath, text, Encoding.UTF8);
                _logger.LogDebug("Updated last processed date to {Date:o} at {Path}.", dtUtc, filePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to write last processed date to {Path}.", filePath);
            }
        }

        public async Task SaveCollectorConnectionsAsync(List<ConnectionInfo> connectionsInfo, string collectorInfoOutputFilePath)
        {

            if (connectionsInfo == null)
            {
                throw new ArgumentNullException(nameof(connectionsInfo));
            }

            await _fileLock.WaitAsync();
            try
            {
                var filePath = Path.Combine(_executionOptions.ExecutionFolder, collectorInfoOutputFilePath);
                await using var fileStream = File.Create(filePath);
                await JsonSerializer.SerializeAsync(fileStream, connectionsInfo, _jsonOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving page state for search");
                throw;
            }
            finally
            {
                _fileLock.Release();
            }
        }

        private static readonly JsonSerializerOptions _jsonlOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
        public async Task<ISet<string>> LoadProcessedUrlsAsync(string searchText)
        {
            var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var file = Path.Combine(_executionOptions.ExecutionFolder, $"processed.jsonl");
            if (!File.Exists(file))
            {
                return set;
            }

            await _fileLock.WaitAsync();
            try
            {
                await foreach (var line in File.ReadLinesAsync(file))
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    try
                    {
                        var rec = JsonSerializer.Deserialize<ProcessedProfileRecord>(line, _jsonlOptions);
                        if (!string.IsNullOrWhiteSpace(rec?.Url))
                            set.Add(rec!.Url);
                    }
                    catch
                    {
                        // ignore bad lines but keep going
                    }
                }
                _logger.LogInformation("📖 [Tracking] Loaded {Count} processed URL(s) from {File}", set.Count, file);
            }
            finally
            {
                _fileLock.Release();
            }
            return set;
        }

        public async Task<string> SaveProfileJsonAsync(string searchText, LinkedInProfile profile, string folderPath)
        {
            Directory.CreateDirectory(folderPath);

            // filename: <normalized-name or 'profile'>_<yyyyMMdd_HHmmss>.json
            var baseName = !string.IsNullOrWhiteSpace(profile.FullName)
                ? StringHelpers.NormalizeWords(profile.FullName)
                : "profile";

            var stamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
            var fileName = $"{baseName}_{stamp}.json";
            var path = Path.Combine(folderPath, fileName);

            await _fileLock.WaitAsync();
            try
            {
                await File.WriteAllTextAsync(path,
                    JsonSerializer.Serialize(profile, new JsonSerializerOptions { WriteIndented = true }));
                _logger.LogInformation("💾 [Tracking] Saved profile JSON: {Path}", path);
            }
            finally
            {
                _fileLock.Release();
            }

            return path;
        }
        //List<ConversationThread>

        public async Task<List<ConversationThread>> LoadConversationThreadAsync(string conversationOutputFilePath)
        {
            // Construye la ruta una sola vez y úsala en todos los logs
            var filePath = Path.Combine(_executionOptions.ExecutionFolder, conversationOutputFilePath);

            try
            {
                if (!File.Exists(filePath))
                {
                    _logger.LogWarning(
                        "Conversation threads file not found at {FilePath}. Returning an empty list.",
                        filePath);
                    return [];
                }

                await using var fileStream = File.OpenRead(filePath);
                var threads = await JsonSerializer.DeserializeAsync<List<ConversationThread>>(fileStream, _jsonOptions);

                if (threads is null)
                {
                    _logger.LogWarning(
                        "Conversation threads file at {FilePath} deserialized to null. Returning an empty list.",
                        filePath);
                    return [];
                }

                _logger.LogDebug(
                    "Loaded {Count} conversation threads from {FilePath}.",
                    threads.Count, filePath);

                return threads;
            }
            catch (JsonException jex)
            {
                _logger.LogError(jex,
                    "Invalid JSON format in conversation threads file at {FilePath}. Returning an empty list.",
                    filePath);
                return [];
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Unexpected error loading conversation threads from {FilePath}.",
                    filePath);
                throw;
            }
        }

        public async Task SaveConversationThreadAsync(List<ConversationThread> threads, string conversationOutputFilePath)
        {
            if (string.IsNullOrWhiteSpace(conversationOutputFilePath))
            {
                throw new ArgumentException(
                    "Conversation threads file path cannot be null or whitespace.",
                    nameof(conversationOutputFilePath));
            }

            var filePath = Path.Combine(_executionOptions.ExecutionFolder, conversationOutputFilePath);

            await _fileLock.WaitAsync();
            try
            {
                // Ensure directory exists
                var directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                    _logger.LogInformation("Created directory for conversation threads at {Directory}.", directory);
                }

                var existed = File.Exists(filePath);
                _logger.LogInformation(
                    existed
                        ? "Overwriting conversation threads file at {FilePath}."
                        : "Creating new conversation threads file at {FilePath}.",
                    filePath);

                await using var fileStream = File.Create(filePath);
                var list = threads ?? [];
                await JsonSerializer.SerializeAsync(fileStream, list, _jsonOptions);

                _logger.LogInformation("Saved {Count} conversation threads to {FilePath}.", list.Count, filePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving conversation threads to {FilePath}.", filePath);
                throw;
            }
            finally
            {
                _fileLock.Release();
            }
        }


        public async Task<List<LinkedInProfile>> LoadDetailedProfilesAsync(string detailedProfilesOutputFilePath)
        {
            try
            {
                var filePath = Path.Combine(_executionOptions.ExecutionFolder, detailedProfilesOutputFilePath);

                if (!File.Exists(filePath))
                {
                    _logger.LogWarning(
                        "⚠️ Detailed profiles file not found at path: {FilePath}. Returning an empty list.",
                        filePath);
                    return [];
                }

                await using var fileStream = File.OpenRead(filePath);
                var profiles = await JsonSerializer.DeserializeAsync<List<LinkedInProfile>>(fileStream, _jsonOptions);
                return profiles ?? [];
            }
            catch (JsonException jex)
            {
                _logger.LogError(jex,
                    "❌ Invalid JSON format in detailed profiles file: {FilePath}. Returning an empty list.",
                    detailedProfilesOutputFilePath);
                return [];
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "❌ Unexpected error loading detailed LinkedIn profiles from file: {FilePath}",
                    detailedProfilesOutputFilePath);
                throw;
            }
        }


        public async Task SaveLinkedInProfilesAsync(List<LinkedInProfile> detailed, string detailedProfilesOutputFilePath)
        {
            if (string.IsNullOrWhiteSpace(detailedProfilesOutputFilePath))
            {
                throw new ArgumentException(
                    "Detailed profiles file path cannot be null or whitespace",
                    nameof(detailedProfilesOutputFilePath));
            }

            await _fileLock.WaitAsync();
            try
            {
                //var searchId = GenerateSearchId(detailedProfilesOutputFilePath);
                var filePath = Path.Combine(_executionOptions.ExecutionFolder, detailedProfilesOutputFilePath);

                // Ensure directory exists
                var directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                    _logger.LogInformation("📁 Created directory for detailed profiles: {Directory}", directory);
                }

                // Create file if it doesn't exist
                if (!File.Exists(filePath))
                {
                    _logger.LogInformation("📄 Creating new detailed profiles file at: {FilePath}", filePath);
                }

                await using var fileStream = File.Create(filePath);
                await JsonSerializer.SerializeAsync(fileStream, detailed ?? [], _jsonOptions);

                _logger.LogInformation(
                    "✅ Successfully saved {Count} detailed LinkedIn profiles to file: {FilePath}",
                    detailed?.Count ?? 0, filePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "❌ Error saving detailed LinkedIn profiles to file: {FilePath}",
                    detailedProfilesOutputFilePath);
                throw;
            }
            finally
            {
                _fileLock.Release();
            }
        }

    }
}