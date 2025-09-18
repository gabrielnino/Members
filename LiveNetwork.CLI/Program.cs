using System.Reflection;
using Api.Startup;
using Commands;
using Configuration;
using LiveNetwork.CLI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Persistence.Context.Implementation;
using Serilog;

public class Program : Builder
{
    public static class Program
    {
        // Lista de flags válidos
        private static readonly string[] KnownFlags = new[]
        {
            "--prompt", "--invite", "--load", "--chat", "--help"
        };

        public static async Task<int> Main(string[] args)
        {
            // Si hay flags conocidos, continúa con el flujo normal del CLI
            if (ContainsKnownFlag(args))
                CommandBootstrap.RunAsync(args);

            // Sin argumentos válidos -> mostrar menú
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
                Console.WriteLine("----------------------------------------");
                Console.WriteLine(" 0) Exit");
                Console.WriteLine("========================================");
                Console.Write("Select an option: ");

                var key = Console.ReadKey(intercept: true).KeyChar;
                Console.WriteLine();

                string[] selectedArgs = key switch
                {
                    '1' => new[] { "--prompt" },
                    '2' => new[] { "--invite" },
                    '3' => new[] { "--load" },
                    '4' => new[] { "--chat" },
                    '0' => Array.Empty<string>(),
                    _ => null!
                };

                if (selectedArgs is null)
                {
                    Console.WriteLine("Invalid option. Press any key to continue...");
                    Console.ReadKey();
                    continue;
                }

                if (selectedArgs.Length == 0)
                {
                    // Exit
                    Console.WriteLine("Bye!");
                    return 0;
                }

                try
                {
                    var code = CommandBootstrap.RunAsync(selectedArgs);
                    Console.WriteLine();
                    Console.WriteLine($"Command finished with exit code {code}.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine();
                    Console.WriteLine("An error occurred while running the command:");
                    Console.WriteLine(ex.Message);
                }

                Console.WriteLine();
                Console.Write("Press any key to return to the menu...");
                Console.ReadKey();
            }
        }

        private static bool ContainsKnownFlag(string[] args)
        {
            if (args is null || args.Length == 0) return false;
            foreach (var a in args)
            {
                foreach (var f in KnownFlags)
                {
                    if (string.Equals(a, f, StringComparison.OrdinalIgnoreCase))
                        return true;
                }
            }
            return false;
        }
    }
    }
