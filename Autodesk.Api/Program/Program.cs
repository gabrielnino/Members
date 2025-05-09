namespace Api.Program
{
    using Autodesk.Api.Program;
    using Autodesk.Persistence.Context;
    using System.Text.RegularExpressions;

    internal class Program : Services
    {
        private static void Main(string[] args)
        {
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
            builder.Services.AddControllers();
            ConfigureServices(builder);
            WebApplication app = builder.Build();
            ConfigureMiddleware(app);
            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<DataContext>();
                if (!db.Initialize())
                {
                    // initialization failed — you could log and/or stop the app
                    throw new Exception("Database initialization failed");
                }
            }
            var services = builder.Services;
            foreach (var service in services)
            {
                var serviceNameFull = service.ServiceType.FullName ?? "Unknown";
                var serviceName = ExtractSimpleTypeName(serviceNameFull);
                var implementationFull = service.ImplementationType?.FullName ?? "Unknown";
                var implementation = ExtractSimpleTypeName(implementationFull);
                var lifetime = service.Lifetime;
                if (serviceNameFull.StartsWith("Application.UseCases"))
                {
                    Console.WriteLine($"Service: {serviceName}, Implementation: {implementation}, Lifetime: {lifetime}");
                }
            }

            app.Run();
        }

        static string ExtractSimpleTypeName(string input)
        {
            var regex = new Regex(@"^(?:.*\.)?(?<TypeName>\w+)(?:`\d+)?\[(?<GenericArgs>.*)\]");
            var match = regex.Match(input);
            if (match.Success)
            {
                var typeName = match.Groups["TypeName"].Value;
                var genericArgs = match.Groups["GenericArgs"].Value;
                var args = SplitGenericArguments(genericArgs);
                var simplifiedArgs = args.Select(arg =>
                {
                    var commaIndex = arg.IndexOf(",", StringComparison.Ordinal);
                    if (commaIndex > 0)
                        arg = arg.Substring(0, commaIndex);
                    return ExtractSimpleTypeName(arg);
                });
                return $"{typeName}[{string.Join(", ", simplifiedArgs)}]";
            }
            else
            {
                return input.Split('.').Last();
            }
        }

        private static List<string> SplitGenericArguments(string input)
        {
            var args = new List<string>();
            var bracketLevel = 0;
            var lastIndex = 0;
            for (var i = 0; i < input.Length; i++)
            {
                switch (input[i])
                {
                    case '[':
                        bracketLevel++;
                        break;
                    case ']':
                        bracketLevel--;
                        break;
                    case ',' when bracketLevel == 0:
                        args.Add(input.Substring(lastIndex, i - lastIndex));
                        lastIndex = i + 1;
                        break;
                }
            }

            if (lastIndex < input.Length)
                args.Add(input.Substring(lastIndex));
            return args;
        }
    }
}