using Application.Result;
using System.Collections.Concurrent;
using System.Text.Json;

namespace Infrastructure.Result
{
    public class ErrorStrategyHandler : IErrorStrategyHandler
    {
        private static readonly Lazy<ConcurrentDictionary<string, string>> ErrorMappings 
            = new(() => new ConcurrentDictionary<string, string>());

        private static readonly IDictionary<string, string> DefaultMappings = new Dictionary<string, string>
        {
            { "SqliteException",           nameof(DatabaseStrategy<object>)       },
            { "HttpRequestException",      nameof(NetworkErrorStrategy<object>)  },
            { "JsonException",             nameof(InvalidDataStrategy<object>)   },
            { "Exception",                 nameof(UnexpectedErrorStrategy<object>) }
        };

        public Operation<T> Fail<T>(Exception? ex, string errorMessage)
        {
            if (ex == null)
            {
                return new NullExceptionStrategy<T>().CreateFailure("Exception is null.");
            }

            if (ErrorMappings.Value.IsEmpty)
            {
                return new NullExceptionStrategy<T>().CreateFailure("ErrorMappings is not loaded or empty.");
            }

            if (!ErrorMappings.Value.TryGetValue(ex.GetType().Name, out var strategyName))
            {
                return new NullExceptionStrategy<T>().CreateFailure($"No strategy matches exception type: {ex.GetType().Name}.");
            }

            var strategy = CreateStrategyInstance<T>(strategyName);

            return strategy.CreateFailure(errorMessage);
        }

        public Operation<T> Fail<T>(Exception? ex)
        {
            if (ex == null)
            {
                return new NullExceptionStrategy<T>().CreateFailure("Exception is null.");
            }

            if (ErrorMappings.Value.IsEmpty)
            {
                return new NullExceptionStrategy<T>().CreateFailure("ErrorMappings is not loaded or empty.");
            }

            if (!ErrorMappings.Value.TryGetValue(ex.GetType().Name, out var strategyName))
            {
                return new NullExceptionStrategy<T>().CreateFailure($"No strategy matches exception type: {ex.GetType().Name}.");
            }

            var strategy = CreateStrategyInstance<T>(strategyName);

            return strategy.CreateFailure();
        }

        private static IErrorCreationStrategy<T> CreateStrategyInstance<T>(string strategyName) =>
            strategyName switch
            {
                "NetworkErrorStrategy" => new NetworkErrorStrategy<T>(),
                "ConfigMissingStrategy" => new ConfigMissingStrategy<T>(),
                "InvalidDataStrategy" => new InvalidDataStrategy<T>(),
                "DatabaseStrategy" => new DatabaseStrategy<T>(),
                "UnexpectedErrorStrategy" => new UnexpectedErrorStrategy<T>(),
                _ => new UnexpectedErrorStrategy<T>() // Default strategy
            };

        public void LoadErrorMappings(string filePath)
        {
            foreach (var kv in DefaultMappings)
            {
                ErrorMappings.Value[kv.Key] = kv.Value;
            }

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Error mappings file not found: {filePath}");
            }

            try
            {
                string jsonContent = File.ReadAllText(filePath);
                var mappings = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonContent);

                if (mappings == null || mappings.Count == 0)
                {
                    throw new InvalidOperationException("ErrorMappings.json is empty or invalid.");
                }

                foreach (var kvp in mappings)
                {
                    ErrorMappings.Value[kvp.Key] = kvp.Value;
                }
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException("ErrorMappings.json contains invalid JSON format.", ex);
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while loading error mappings.", ex);
            }
        }

        public Operation<T> Business<T>(string errorMessage)
        {
            return new BusinessStrategy<T>().CreateFailure(errorMessage);
        }

        public bool Any() => ErrorMappings.IsValueCreated && ErrorMappings.Value.Count > 0;

    }
}
