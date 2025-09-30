using System.Diagnostics;
using System.Reflection;
using Api.Startup;
using Commands;
using Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Persistence.Context.Implementation;
using Serilog;

namespace LiveNetwork.CLI
{
    internal class CommandBootstrap : Builder
    {
        // ==== Flags admitidos por el menú / línea de comandos ====
        private static readonly string[] KnownFlags = new[]
        {
            "--prompt", "--invite", "--load", "--chat", "--search", "--scrape-reviews", "--help"
        };

        public static HostApplicationBuilder CreateDefaultAppBuilder(string[] args, string? basePath = null)
        {
            var cmdArgs = (args is { Length: > 0 }) ? args : Array.Empty<string>();
            var builder = Host.CreateApplicationBuilder(cmdArgs); // HostApplicationBuilder

            basePath ??= Directory.GetCurrentDirectory();
            var envName = builder.Environment.EnvironmentName;

            builder.Configuration
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{envName}.json", optional: true, reloadOnChange: true);
            if (builder.Environment.IsDevelopment())
                builder.Configuration.AddUserSecrets(Assembly.GetExecutingAssembly(), optional: true);
            builder.Configuration.AddEnvironmentVariables();
            if (cmdArgs.Length > 0) builder.Configuration.AddCommandLine(cmdArgs);
            var executionOptions = new ExecutionTracker(Environment.CurrentDirectory);
            var logPath = Path.Combine(executionOptions.ExecutionFolder, "Logs");
            Directory.CreateDirectory(logPath);
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(builder.Configuration)
                .Enrich.FromLogContext()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .WriteTo.File(
                    path: Path.Combine(logPath, "LiveNetwork-.log"),
                    rollingInterval: RollingInterval.Day,
                    fileSizeLimitBytes: 5_000_000,
                    retainedFileCountLimit: 7,
                    rollOnFileSizeLimit: true,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level}] {Message}{NewLine}{Exception}"
                )
                .CreateLogger();
            builder.Logging.ClearProviders();
            builder.Logging.AddSerilog(Log.Logger, dispose: true);

            return builder;
        }

        public static async Task<int> RunAsync(string[] args, bool enableInteractiveMenu = true)
        {
            using var cts = new CancellationTokenSource();
            Console.CancelKeyPress += (_, e) =>
            {
                e.Cancel = true;
                cts.Cancel();
                Log.Warning("Cancellation requested (Ctrl+C)...");
            };

            try
            {
                var sw = Stopwatch.StartNew();
                Log.Information("🚀 LiveNetwork.CLI started at {Time}", DateTimeOffset.Now);
                var effectiveArgs = ResolveArgsWithMenuIfNeeded(args, enableInteractiveMenu);
                if (effectiveArgs is null)
                {
                    Log.Information("User aborted before execution.");
                    return 0;
                }
                var appBuilder = CreateDefaultAppBuilder(effectiveArgs);
                ConfigureServices(appBuilder, effectiveArgs);
                using var host = appBuilder.Build();
                using (var scope = host.Services.CreateScope())
                {
                    var db = scope.ServiceProvider.GetRequiredService<DataContext>();
                    if (!db.Initialize())
                        throw new Exception("Database initialization failed");
                }
                var exitCode = await ExecuteCommandsAsync(host, cts.Token);
                sw.Stop();
                Log.Information("✅ Done in {ElapsedMs} ms with exit code {ExitCode}", sw.ElapsedMilliseconds, exitCode);
                return exitCode;
            }
            catch (OperationCanceledException)
            {
                Log.Warning("❗ Operation canceled by user.");
                return 2;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "❌ Application terminated unexpectedly");
                return 1;
            }
            finally
            {
                await Log.CloseAndFlushAsync();
            }
        }

        private static string[]? ResolveArgsWithMenuIfNeeded(string[] args, bool enableInteractiveMenu)
        {
            if (ContainsKnownFlag(args))
                return args;

            if (!enableInteractiveMenu)
                return args;

            // Mostrar menú
            while (true)
            {
                Console.Clear();
                Console.WriteLine("========================================");
                Console.WriteLine("          LiveNetwork.CLI  Menu          ");
                Console.WriteLine("========================================");
                Console.WriteLine(" 1) Prompt        (equivale a --prompt)");
                Console.WriteLine(" 2) Invite        (equivale a --invite)");
                Console.WriteLine(" 3) Load          (equivale a --load)");
                Console.WriteLine(" 4) Chat          (equivale a --chat)");
                Console.WriteLine(" 4) Chat          (equivale a --chat)");
                Console.WriteLine(" 5) Search          (equivale a --search)");
                Console.WriteLine(" 6) Scrape          (equivale a --scrape-reviews)");
                Console.WriteLine("----------------------------------------");
                Console.WriteLine(" h) Help          (equivale a --help)");
                Console.WriteLine(" 0) Exit");
                Console.WriteLine("========================================");
                Console.Write("Select an option: ");

                var key = Console.ReadKey(intercept: true).KeyChar;
                Console.WriteLine();

                var selectedArgs = key switch
                {
                    '1' => ["--prompt"],
                    '2' => ["--invite"],
                    '3' => ["--load"],
                    '4' => ["--chat"],
                    '5' => ["--search"],
                    '6' => ["--scrape - reviews"],
                    'h' or 'H' or '?' => new[] { "--help" },
                    '0' => null, // exit
                    _ => Array.Empty<string>()
                };

                if (selectedArgs is null)
                    return null; // usuario eligió salir

                if (selectedArgs.Length == 0)
                {
                    Console.WriteLine("Invalid option. Press any key to continue...");
                    Console.ReadKey();
                    continue;
                }

                return selectedArgs;
            }
        }

        private static bool ContainsKnownFlag(string[]? args)
        {
            if (args is null || args.Length == 0) return false;
            foreach (var a in args)
                foreach (var f in KnownFlags)
                    if (string.Equals(a, f, StringComparison.OrdinalIgnoreCase))
                        return true;
            return false;
        }

        private static async Task<int> ExecuteCommandsAsync(IHost host, CancellationToken ct)
        {
            var commandFactory = host.Services.GetRequiredService<CommandFactory>();
            var jobArgs = host.Services.GetRequiredService<CommandArgs>();

            var commands = commandFactory.CreateCommand().ToList();
            if (commands.Count == 0)
            {
                Log.Warning("No commands were produced by CommandFactory for args: {Args}", string.Join(' ', jobArgs.Arguments));
                return 0;
            }

            var aggregateExit = 0;
            foreach (var command in commands)
            {
                ct.ThrowIfCancellationRequested();

                try
                {
                    Log.Information("▶ Executing command {Command}...", command.GetType().Name);
                    await command.ExecuteAsync(jobArgs.Arguments); ;

                }
                catch (OperationCanceledException)
                {
                    Log.Warning("⏹ {Command} cancelled by user", command.GetType().Name);
                    throw;
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "💥 Execution failed for {Command}", command.GetType().Name);
                    throw;
                }
            }

            return aggregateExit;
        }
    }
}
