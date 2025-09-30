using LiveNetwork.CLI;
using Api.Startup;

public class Program : Builder
{
    private static readonly string[] KnownFlags =
    [
        "--prompt", "--invite", "--load", "--chat", "--search", "--scrape-reviews", "--help", "--version"
    ];

    private static int _previousSelection = 0;
    private static bool _firstDraw = true;
    private static int[] _optionLinePositions = Array.Empty<int>();

    public static async Task<int> Main(string[] args)
    {
        if (ContainsKnownFlag(args))
        {
            return await CommandBootstrap.RunAsync(args);
        }

        Console.CursorVisible = false;
        Console.Title = "LiveNetwork.CLI - Interactive Menu";

        // Initialize the console
        InitializeConsole();

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
                6 => ["--scrape-reviews"],
                7 => ["--help"],
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

        // Header
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.WriteLine("  Navigate with ↑↓ arrows or number keys 1-7 • Enter to select • Esc to exit");
        Console.ResetColor();
        Console.WriteLine();

        string[] options =
        [
            "1. Generate AI Messages     (--prompt)          - Create personalized outreach content",
            "2. Send Invitations         (--invite)          - Automate connection requests",
            "3. Load Connections         (--load)            - Collect network data & profiles",
            "4. Automated Messaging      (--chat)            - Engage in conversation threads",
            "5. Search Connections       (--search)          - Find and filter your network",
            "6. Scrape Reviews           (--scrape-reviews)  - Extract product reviews from sites",
            "7. Help & Documentation     (--help)            - View detailed instructions",
            "0. Exit Application"
        ];

        string[] descriptions =
        [
            "  • AI-powered message generation for personalized outreach campaigns",
            "  • Automated connection invitation sending to targeted prospects",
            "  • Data collection and analysis from your existing LinkedIn network",
            "  • Automated follow-up messaging and conversation management",
            "  • Advanced search capabilities to find specific connections",
            "  • Scrape and export software reviews (Capterra/G2) into CSV for pain analysis",
            "  • Command reference, usage examples, and troubleshooting",
            "  • Safely close the application"
        ];

        // Prepare line positions for selection rendering
        _optionLinePositions = new int[options.Length];

        for (int i = 0; i < options.Length; i++)
        {
            // Separator before Exit
            if (i == options.Length - 1)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("  ───────────────────────────────────────────────────────────────────────────");
                Console.ResetColor();
            }

            _optionLinePositions[i] = Console.CursorTop;

            bool isSelected = (i == _previousSelection) && !_firstDraw;

            if (isSelected)
            {
                Console.BackgroundColor = ConsoleColor.DarkBlue;
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($" {options[i]}");
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine($" {descriptions[i]}");
                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = (i == options.Length - 1) ? ConsoleColor.Red : ConsoleColor.White;
                Console.WriteLine($" {options[i]}");
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine($" {descriptions[i]}");
                Console.ResetColor();
            }

            Console.WriteLine();
        }

        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine("  Press F1 for help, Home/End to jump to first/last option");
        Console.ResetColor();

        _firstDraw = false;
    }

    private static int HandleMenuNavigation()
    {
        // There are 8 visible lines (1..7 + Exit), indexes 0..7 in arrays
        int selectedIndex = _previousSelection;
        int totalOptions = 8;

        // Position cursor to current selection
        SetCursorPosition(0, _optionLinePositions[selectedIndex]);

        while (true)
        {
            var key = Console.ReadKey(true).Key;

            switch (key)
            {
                case ConsoleKey.UpArrow:
                    selectedIndex = (selectedIndex - 1 + totalOptions) % totalOptions;
                    UpdateSelection(selectedIndex);
                    break;

                case ConsoleKey.DownArrow:
                    selectedIndex = (selectedIndex + 1) % totalOptions;
                    UpdateSelection(selectedIndex);
                    break;

                case ConsoleKey.Home:
                    selectedIndex = 0;
                    UpdateSelection(selectedIndex);
                    break;

                case ConsoleKey.End:
                    selectedIndex = totalOptions - 1;
                    UpdateSelection(selectedIndex);
                    break;

                case ConsoleKey.Enter:
                    _previousSelection = selectedIndex;
                    // Map visual index -> return code
                    return IndexToSelection(selectedIndex);

                case ConsoleKey.Escape:
                    return 0;

                case ConsoleKey.F1:
                    ShowHelp();
                    DrawMenu();
                    break;

                case ConsoleKey.D1:
                case ConsoleKey.NumPad1:
                    _previousSelection = 0; return 1;
                case ConsoleKey.D2:
                case ConsoleKey.NumPad2:
                    _previousSelection = 1; return 2;
                case ConsoleKey.D3:
                case ConsoleKey.NumPad3:
                    _previousSelection = 2; return 3;
                case ConsoleKey.D4:
                case ConsoleKey.NumPad4:
                    _previousSelection = 3; return 4;
                case ConsoleKey.D5:
                case ConsoleKey.NumPad5:
                    _previousSelection = 4; return 5;
                case ConsoleKey.D6:
                case ConsoleKey.NumPad6:
                    _previousSelection = 5; return 6;
                case ConsoleKey.D7:
                case ConsoleKey.NumPad7:
                    _previousSelection = 6; return 7;
                case ConsoleKey.D0:
                case ConsoleKey.NumPad0:
                    _previousSelection = 7; return 0;
            }
        }
    }

    private static int IndexToSelection(int index)
    {
        // index 0..7 map to: 1..7 and Exit(0)
        return index switch
        {
            0 => 1, // --prompt
            1 => 2, // --invite
            2 => 3, // --load
            3 => 4, // --chat
            4 => 5, // --search
            5 => 6, // --scrape-reviews
            6 => 7, // --help
            7 => 0, // Exit
            _ => 0
        };
    }

    private static void UpdateSelection(int selectedIndex)
    {
        int totalOptions = 8;

        for (int i = 0; i < totalOptions; i++)
        {
            SetCursorPosition(0, _optionLinePositions[i]);

            bool isSelected = (i == selectedIndex);

            if (isSelected)
            {
                Console.BackgroundColor = ConsoleColor.DarkBlue;
                Console.ForegroundColor = ConsoleColor.White;

                string optionText = GetOptionText(i);
                Console.Write(" " + optionText);
                Console.Write(new string(' ', Math.Max(0, Console.WindowWidth - optionText.Length - 2)));

                Console.ResetColor();
            }
            else
            {
                Console.ResetColor();

                string optionText = GetOptionText(i);
                Console.ForegroundColor = (i == totalOptions - 1) ? ConsoleColor.Red : ConsoleColor.White;
                Console.Write(" " + optionText);
                Console.Write(new string(' ', Math.Max(0, Console.WindowWidth - optionText.Length - 2)));

                Console.ResetColor();
            }
        }

        SetCursorPosition(0, _optionLinePositions[selectedIndex]);
        _previousSelection = selectedIndex;
    }

    private static string GetOptionText(int index)
    {
        return index switch
        {
            0 => "1. Generate AI Messages     (--prompt)          - Create personalized outreach content",
            1 => "2. Send Invitations         (--invite)          - Automate connection requests",
            2 => "3. Load Connections         (--load)            - Collect network data & profiles",
            3 => "4. Automated Messaging      (--chat)            - Engage in conversation threads",
            4 => "5. Search Connections       (--search)          - Find and filter your network",
            5 => "6. Scrape Reviews           (--scrape-reviews)  - Extract product reviews from sites",
            6 => "7. Help & Documentation     (--help)            - View detailed instructions",
            7 => "0. Exit Application",
            _ => ""
        };
    }

    private static async Task ExecuteCommand(string[] args)
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("╔══════════════════════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║                             EXECUTING COMMAND                                   ║");
        Console.WriteLine("╚══════════════════════════════════════════════════════════════════════════════════╝");
        Console.ResetColor();
        Console.WriteLine();
        Console.WriteLine($"  • Command: {args[0]}");
        Console.WriteLine("──────────────────────────────────────────────────────────────────────────────────");

        try
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var exitCode = await CommandBootstrap.RunAsync(args);
            stopwatch.Stop();

            Console.WriteLine();
            Console.ForegroundColor = exitCode == 0 ? ConsoleColor.Green : ConsoleColor.Yellow;
            Console.WriteLine($"  • Command completed in {stopwatch.Elapsed.TotalSeconds:F2}s");
            Console.WriteLine($"Exit code: {exitCode}");
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("  • An error occurred:");
            Console.WriteLine(ex.Message);
            Console.ResetColor();
            Console.WriteLine();
            Console.WriteLine("Stack trace:");
            Console.WriteLine(ex.StackTrace);
        }

        Console.ResetColor();
        Console.WriteLine();
        Console.WriteLine("Press any key to return to menu...");
        Console.ReadKey(true);
    }

    private static void ShowHelp()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("╔══════════════════════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║                                HELP & DOCUMENTATION                             ║");
        Console.WriteLine("╚══════════════════════════════════════════════════════════════════════════════════╝");
        Console.ResetColor();

        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("Available Commands:");
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.WriteLine("• --prompt          : Generate AI-powered personalized messages for outreach campaigns");
        Console.WriteLine("• --invite          : Automate connection requests with customizable templates");
        Console.WriteLine("• --load            : Extract and analyze your LinkedIn network data");
        Console.WriteLine("• --chat            : Manage automated messaging with your connections");
        Console.WriteLine("• --search          : Advanced search through your network with filters");
        Console.WriteLine("• --scrape-reviews  : Scrape software reviews (Capterra/G2) into CSV for pain analysis");
        Console.WriteLine("• --help            : Show this help");
        Console.WriteLine("• --version         : Show CLI version");
        Console.WriteLine();

        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("Scrape Reviews Usage:");
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.WriteLine("  LiveNetwork.CLI --scrape-reviews --site <capterra|g2> --product <name> --max <int> --out <path>");
        Console.WriteLine("  Examples:");
        Console.WriteLine("    LiveNetwork.CLI --scrape-reviews --site capterra --product quickbooks --max 50 --out ./qbo_reviews.csv");
        Console.WriteLine("    LiveNetwork.CLI --scrape-reviews --product clio");
        Console.WriteLine("  Notes:");
        Console.WriteLine("    • --site defaults to 'capterra'");
        Console.WriteLine("    • --product keys: quickbooks|xero|freshbooks|clio|mycase|practicepanther|toggl|harvest");
        Console.WriteLine("    • --out defaults to './reviews.csv'");
        Console.WriteLine();

        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("Navigation Shortcuts:");
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.WriteLine("• Arrow keys: Navigate through menu options");
        Console.WriteLine("• Number keys: Direct selection (0-7)");
        Console.WriteLine("• Home/End: Jump to first/last option");
        Console.WriteLine("• Enter: Select highlighted option");
        Console.WriteLine("• Esc: Exit application");
        Console.WriteLine("• F1: Show this help screen");
        Console.WriteLine();

        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("Tips:");
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.WriteLine("• Use --help with any command for command-specific help");
        Console.WriteLine("• Ensure your Selenium WebDriver is configured (and logged in if needed) before scraping");
        Console.WriteLine("• Start with a small --max value to validate selectors");
        Console.WriteLine();

        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.Write("Press any key to continue...");
        Console.ResetColor();
        Console.ReadKey(true);
    }

    private static void ShowExitAnimation()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("╔══════════════════════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║                             THANK YOU FOR USING                                 ║");
        Console.WriteLine("╚══════════════════════════════════════════════════════════════════════════════════╝");
        Console.ResetColor();
        Console.WriteLine();

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("  • Thank you for using LiveNetwork CLI!");
        Console.ResetColor();
        Console.WriteLine();
        Console.Write("Shutting down");

        for (int i = 0; i < 5; i++)
        {
            Console.Write(".");
            Thread.Sleep(200);
        }
    }

    private static bool ContainsKnownFlag(string[] args)
    {
        if (args is null || args.Length == 0) return false;
        return args.Any(arg => KnownFlags.Contains(arg, StringComparer.OrdinalIgnoreCase));
    }

    public static void InitializeConsole()
    {
        Console.CursorVisible = false;
        Console.Title = "LiveNetwork CLI";
    }

    public static void SetCursorPosition(int left, int top)
    {
        Console.SetCursorPosition(left, top);
    }
}
