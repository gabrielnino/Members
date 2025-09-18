using Api.Startup;
using LiveNetwork.CLI;
using System.Drawing;

public class Program : Builder
{
    private static readonly string[] KnownFlags = new[]
    {
        "--prompt", "--invite", "--load", "--chat", "--search", "--help"
    };

    public static async Task<int> Main(string[] args)
    {
        if (ContainsKnownFlag(args))
        {
            return await CommandBootstrap.RunAsync(args);
        }

        Console.CursorVisible = false;
        Console.Title = "LiveNetwork.CLI - Interactive Menu";

        while (true)
        {
            DrawMenu();
            var selectedOption = HandleMenuNavigation();

            if (selectedOption == 0) // Exit
            {
                ShowExitAnimation();
                return 0;
            }

            string[] selectedArgs = selectedOption switch
            {
                1 => ["--prompt"],
                2 => ["--invite"],
                3 => ["--load"],
                4 => ["--chat"],
                5 => ["--search"],
                _ => null!
            };

            if (selectedArgs != null)
            {
                await ExecuteCommand(selectedArgs);
            }
        }
    }

    private static void DrawMenu()
    {
        Console.Clear();
        Console.ResetColor();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("╔══════════════════════════════════════════════════════════╗");
        Console.WriteLine("║                LinkedIn Automation Toolkit               ║");
        Console.WriteLine("╚══════════════════════════════════════════════════════════╝");
        Console.ResetColor();
        Console.WriteLine();
        Console.WriteLine("  Use ↑↓ arrows to navigate, Enter to select");
        Console.WriteLine();

        string[] options = {
        "Generate AI Messages     (--prompt) - Create personalized outreach content",
        "Send Invitations         (--invite) - Automate connection requests",
        "Load Connections         (--load)   - Collect network data & profiles",
        "Automated Messaging      (--chat)   - Engage in conversation threads",
        "Search connection     (--search)   - search",
        "Exit Application"
    };

        string[] descriptions = {
        "  • AI-powered message generation for personalized outreach",
        "  • Automated connection invitation sending to prospects",
        "  • Data collection from your existing LinkedIn network",
        "  • Automated follow-up messaging with connections",
        "  • Close the application",
        "  • Search connection "
    };

        for (int i = 0; i < options.Length; i++)
        {
            if (i == 6)
            {
                Console.WriteLine("  ───────────────────────────────────────────────────────");
            }

            Console.ForegroundColor = i == 4 ? ConsoleColor.Red : ConsoleColor.White;
            Console.WriteLine($"  {options[i]}");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"  {descriptions[i]}");
            Console.ResetColor();
            Console.WriteLine();
        }

        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine("  Press F1 for detailed help, Esc to exit");
        Console.ResetColor();
    }

    private static int HandleMenuNavigation()
    {
        int selectedIndex = 0;
        int totalOptions = 6; // 4 commands + exit

        while (true)
        {
            var key = Console.ReadKey(true).Key;

            switch (key)
            {
                case ConsoleKey.UpArrow:
                    selectedIndex = (selectedIndex - 1 + totalOptions) % totalOptions;
                    UpdateSelection(selectedIndex, totalOptions);
                    break;

                case ConsoleKey.DownArrow:
                    selectedIndex = (selectedIndex + 1) % totalOptions;
                    UpdateSelection(selectedIndex, totalOptions);
                    break;

                case ConsoleKey.Enter:
                    return selectedIndex + 1;

                case ConsoleKey.Escape:
                    return 0;

                case ConsoleKey.F1:
                    ShowHelp();
                    DrawMenu();
                    break;

                case ConsoleKey.D1:
                case ConsoleKey.NumPad1:
                    return 1;
                case ConsoleKey.D2:
                case ConsoleKey.NumPad2:
                    return 2;
                case ConsoleKey.D3:
                case ConsoleKey.NumPad3:
                    return 3;
                case ConsoleKey.D4:
                case ConsoleKey.NumPad4:
                    return 4;
                case ConsoleKey.D5:
                case ConsoleKey.NumPad5:
                    return 5;
                case ConsoleKey.D0:
                case ConsoleKey.NumPad0:
                    return 0;
            }
        }
    }

    private static void UpdateSelection(int selectedIndex, int totalOptions)
    {
        Console.SetCursorPosition(0, 6); // Start of options

        string[] options = {
            "  • Prompt        (--prompt)",
            "  • Invite        (--invite)",
            "  • Load          (--load)",
            "  • Chat          (--chat)",
            "  • Search        (--search)",
            "  • Exit"
        };

        for (int i = 0; i < totalOptions; i++)
        {
            Console.SetCursorPosition(0, 6 + i + (i >= 4 ? 1 : 0)); // Account for separator

            if (i == selectedIndex)
            {
                Console.ForegroundColor = ConsoleColor.Black;
                Console.BackgroundColor = ConsoleColor.White;
                Console.Write($"► {options[i]}");
                Console.ResetColor();
                Console.Write(new string(' ', Console.WindowWidth - options[i].Length - 3));
            }
            else
            {
                Console.ResetColor();
                Console.Write($"  {options[i]}");
                Console.Write(new string(' ', Console.WindowWidth - options[i].Length - 2));
            }
        }
    }

    private static async Task ExecuteCommand(string[] args)
    {
        Console.Clear();
        Console.WriteLine($"🚀 Executing: {args[0]}");
        Console.WriteLine("──────────────────────────────────────────");

        try
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var exitCode = await CommandBootstrap.RunAsync(args);
            stopwatch.Stop();

            Console.WriteLine();
            Console.ForegroundColor = exitCode == 0 ? ConsoleColor.Green : ConsoleColor.Red;
            Console.WriteLine($"✅ Command completed in {stopwatch.Elapsed.TotalSeconds:F2}s");
            Console.WriteLine($"Exit code: {exitCode}");
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("❌ An error occurred:");
            Console.WriteLine(ex.Message);
            Console.ResetColor();
            Console.WriteLine();
            Console.WriteLine("Stack trace:");
            Console.WriteLine(ex.StackTrace);
        }

        Console.ResetColor();
        Console.WriteLine();
        Console.WriteLine("Press any key to return to menu...");
        Console.ReadKey();
    }

    private static void ShowHelp()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("📖 LiveNetwork.CLI - Help");
        Console.WriteLine("══════════════════════════════════════════");
        Console.ResetColor();

        Console.WriteLine();
        Console.WriteLine("Available commands:");
        Console.WriteLine("• --prompt   : Interactive prompt mode");
        Console.WriteLine("• --invite   : Invite management");
        Console.WriteLine("• --load     : Load configuration/files");
        Console.WriteLine("• --chat     : Start chat session");
        Console.WriteLine("• --search   : Search connections");
        Console.WriteLine();
        Console.WriteLine("Navigation:");
        Console.WriteLine("• Arrow keys: Navigate menu");
        Console.WriteLine("• Enter     : Select option");
        Console.WriteLine("• Number keys: Quick selection (1-4)");
        Console.WriteLine("• Esc/F10   : Exit program");
        Console.WriteLine("• F1        : Show this help");

        Console.WriteLine();
        Console.Write("Press any key to continue...");
        Console.ReadKey();
    }

    private static void ShowExitAnimation()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("👋 Thank you for using LiveNetwork.CLI!");
        Console.ResetColor();
        Console.WriteLine("Shutting down...");

        for (int i = 0; i < 3; i++)
        {
            Console.Write(".");
            Thread.Sleep(300);
        }
    }

    private static bool ContainsKnownFlag(string[] args)
    {
        if (args is null || args.Length == 0) return false;
        return args.Any(arg => KnownFlags.Contains(arg, StringComparer.OrdinalIgnoreCase));
    }
}