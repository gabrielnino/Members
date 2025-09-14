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

public class Program : Builder
{
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
#if DEBUG
        if (builder.Environment.IsDevelopment())
            builder.Configuration.AddUserSecrets(Assembly.GetExecutingAssembly(), optional: true);
#endif
        builder.Configuration.AddEnvironmentVariables();
        if (cmdArgs.Length > 0) builder.Configuration.AddCommandLine(cmdArgs);

        // AppConfig (bind)
        //var appConfig = new AppConfig();
        //var appSection = builder.Configuration.GetSection("AppConfig");
        //builder.Configuration.Bind(appConfig);
        //builder.Services.AddSingleton(appConfig);

        // ExecutionTracker (para carpeta de logs)
        var executionOptions = new ExecutionTracker(Environment.CurrentDirectory);
        //builder.Services.AddSingleton(executionOptions);

        // Serilog
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

    public static async Task Main(string[] args)
    {
        try
        {
            Log.Information("🚗 Executing booking at {Time}", DateTimeOffset.Now);
            var appBuilder = CreateDefaultAppBuilder(args);
            ConfigureServices(appBuilder, args);

            var host = appBuilder.Build();
            using (var scope = host.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<DataContext>();
                if (!db.Initialize())
                {
                    throw new Exception("Database initialization failed");
                }
            }


            var commandFactory = host.Services.GetRequiredService<CommandFactory>();
            var commands = commandFactory.CreateCommand().ToList();
            var jobArgs = host.Services.GetRequiredService<CommandArgs>();
            foreach (var command in commands)
            {
                try
                {
                    Log.Information("Executing command {Command}...", command.GetType().Name);
                    await command.ExecuteAsync(jobArgs.Arguments);
                    Log.Information("{Command} completed successfully", command.GetType().Name);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Execution failed for {Command}", command.GetType().Name);
                    throw;
                }
            }
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "❌ Application terminated unexpectedly");
            Environment.ExitCode = 1;
        }
        finally
        {
            await Log.CloseAndFlushAsync();
        }

        Log.Information("⏱ Waiting 15 minutes before the next booking attempt...");
    }
}
