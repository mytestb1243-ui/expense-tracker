using First_Console_shafi.Data;
using System.Globalization;

namespace First_Console_shafi;

class Program
{
    private static IExpenseStore Store = null!;
    private static IBudgetStore BudgetStore = null!;
    private static readonly IUserStore UserStore = new JsonFileUserStore("users.json");
    private static readonly ExpenseService Service = new();
    private static readonly AuthService Auth = new();
    private static readonly BudgetService Budgets = new();
    private static readonly CsvExportService CsvExport = new();

    private static List<Expense> _expenses = new();
    private static List<Budget> _budgets = new();
    private static List<User> _users = new();
    private static User _currentUser = null!;

    static async Task Main()
    {
        _users = await UserStore.LoadAsync();

        var appRunning = true;
        while (appRunning)
        {
            _currentUser = await AuthenticateAsync();

            Store = new JsonFileExpenseStore($"expenses.{_currentUser.Username}.json");
            BudgetStore = new JsonFileBudgetStore($"budgets.{_currentUser.Username}.json");
            // async in action: loading from disk doesn't block the thread while it waits on I/O.
            _expenses = await Store.LoadAsync();
            _budgets = await BudgetStore.LoadAsync();
            Console.WriteLine($"Loaded {_expenses.Count} expense(s) for {_currentUser.Username}.\n");

            var sessionRunning = true;
            while (sessionRunning)
            {
                PrintMenu();
                var choice = Console.ReadLine()?.Trim();

                try
                {
                    switch (choice)
                    {
                        case "1": AddExpense(); break;
                        case "2": ViewAll(); break;
                        case "3": ViewSummary(); break;
                        case "4": FilterByCategory(); break;
                        case "5": FilterByDateRange(); break;
                        case "6": SearchExpenses(); break;
                        case "7": EditExpense(); break;
                        case "8": DeleteExpense(); break;
                        case "9": SetBudget(); break;
                        case "10": ViewMonthlyReport(); break;
                        case "11": await ExportToCsvAsync(); break;
                        case "12":
                            await Store.SaveAsync(_expenses);
                            await BudgetStore.SaveAsync(_budgets);
                            Console.WriteLine("Logged out.\n");
                            sessionRunning = false;
                            break;
                        case "13":
                            await Store.SaveAsync(_expenses);
                            await BudgetStore.SaveAsync(_budgets);
                            sessionRunning = false;
                            appRunning = false;
                            break;
                        default: Console.WriteLine("Unknown option.\n"); break;
                    }
                }
                catch (InvalidExpenseException ex)
                {
                    // Custom exception, caught right where it's meaningful to the user.
                    Console.WriteLine($"  Couldn't save that expense: {ex.Message}\n");
                }
                catch (InvalidBudgetException ex)
                {
                    Console.WriteLine($"  Couldn't save that budget: {ex.Message}\n");
                }
            }
        }

        Console.WriteLine("Bye!");
    }

    private static async Task<User> AuthenticateAsync()
    {
        while (true)
        {
            Console.WriteLine("==== Welcome ====");
            Console.WriteLine("1. Log in");
            Console.WriteLine("2. Register");
            Console.WriteLine("3. Exit");
            Console.Write("Choose: ");
            var choice = Console.ReadLine()?.Trim();

            if (choice == "3")
            {
                await UserStore.SaveAsync(_users);
                Console.WriteLine("Bye!");
                Environment.Exit(0);
            }

            if (choice != "1" && choice != "2")
            {
                Console.WriteLine("Unknown option.\n");
                continue;
            }

            Console.Write("Username: ");
            var username = Console.ReadLine() ?? "";
            Console.Write("Password: ");
            var password = ReadPassword();

            try
            {
                var user = choice == "2"
                    ? Auth.Register(_users, username, password)
                    : Auth.Login(_users, username, password);

                if (choice == "2")
                {
                    _users.Add(user);
                    await UserStore.SaveAsync(_users);
                    Console.WriteLine($"Account created. Welcome, {user.Username}!\n");
                }
                else
                {
                    Console.WriteLine($"Welcome back, {user.Username}!\n");
                }

                return user;
            }
            catch (AuthenticationException ex)
            {
                Console.WriteLine($"  {ex.Message}\n");
            }
        }
    }

    // Reads a password from the console without echoing it, showing '*' per character instead.
    // Falls back to plain ReadLine when input is redirected (e.g. piped from a script),
    // since Console.ReadKey requires an interactive console.
    private static string ReadPassword()
    {
        if (Console.IsInputRedirected)
            return Console.ReadLine() ?? "";

        var password = new System.Text.StringBuilder();
        ConsoleKeyInfo key;
        while ((key = Console.ReadKey(intercept: true)).Key != ConsoleKey.Enter)
        {
            if (key.Key == ConsoleKey.Backspace)
            {
                if (password.Length > 0)
                {
                    password.Length--;
                    Console.Write("\b \b");
                }
                continue;
            }

            if (!char.IsControl(key.KeyChar))
            {
                password.Append(key.KeyChar);
                Console.Write('*');
            }
        }
        Console.WriteLine();
        return password.ToString();
    }

    private static void PrintMenu()
    {
        Console.WriteLine("==== Expense Tracker ====");
        Console.WriteLine("1. Add expense");
        Console.WriteLine("2. View all expenses");
        Console.WriteLine("3. View summary");
        Console.WriteLine("4. Filter by category");
        Console.WriteLine("5. Filter by date range");
        Console.WriteLine("6. Search expenses");
        Console.WriteLine("7. Edit an expense");
        Console.WriteLine("8. Delete an expense");
        Console.WriteLine("9. Set monthly budget");
        Console.WriteLine("10. View monthly report");
        Console.WriteLine("11. Export to CSV");
        Console.WriteLine("12. Log out");
        Console.WriteLine("13. Exit");
        Console.Write("Choose: ");
    }

    // Generic helper: reads console input and retries until "parser" accepts it.
    // One method, works for decimal, DateTime, or anything else you hand it — that's generics + delegates together.
    private static T ReadValue<T>(string prompt, Func<string, (bool Success, T Value)> parser)
    {
        while (true)
        {
            Console.Write(prompt);
            var input = Console.ReadLine() ?? string.Empty;
            var (success, value) = parser(input);
            if (success) return value;
            Console.WriteLine("  Invalid input, try again.");
        }
    }

    private static void AddExpense()
    {
        Console.Write("Description: ");
        var description = Console.ReadLine() ?? "";

        var amount = ReadValue("Amount: $", s =>
            decimal.TryParse(s, out var d) && d > 0 ? (true, d) : (false, 0m));

        Console.Write("Category (Food, Transport, Entertainment, Utilities, Shopping, Other): ");
        var categoryInput = Console.ReadLine() ?? "";
        // Switch expression: modern, concise pattern matching instead of a chain of if/else.
        var category = categoryInput.Trim().ToLowerInvariant() switch
        {
            "food" => Category.Food,
            "transport" => Category.Transport,
            "entertainment" => Category.Entertainment,
            "utilities" => Category.Utilities,
            "shopping" => Category.Shopping,
            _ => Category.Other
        };

        var date = ReadValue("Date (yyyy-mm-dd, blank = today): ", s =>
        {
            if (string.IsNullOrWhiteSpace(s)) return (true, DateTime.Today);
            return DateTime.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.None, out var d)
                ? (true, d)
                : (false, DateTime.MinValue);
        });

        Console.Write("Notes (optional): ");
        var notesInput = Console.ReadLine();
        // Nullable reference type: Notes is genuinely optional, and the type system tracks that.
        string? notes = string.IsNullOrWhiteSpace(notesInput) ? null : notesInput;

        var expense = new Expense(Guid.NewGuid(), description, amount, category, date, notes);
        Service.Validate(expense); // throws InvalidExpenseException if something's off

        _expenses.Add(expense);
        Console.WriteLine("Added.");

        var status = Budgets.GetStatus(_expenses, _budgets, category, date);
        if (status is { IsOverBudget: true })
        {
            Console.WriteLine($"  Warning: {category} is now ${status.SpentThisMonth:0.00} of your ${status.Limit:0.00} " +
                               $"monthly budget (over by ${status.SpentThisMonth - status.Limit:0.00}).");
        }
        Console.WriteLine();
    }

    private static void ViewAll()
    {
        if (_expenses.Count == 0)
        {
            Console.WriteLine("No expenses yet.\n");
            return;
        }

        // LINQ: newest first.
        foreach (var e in _expenses.OrderByDescending(e => e.Date))
        {
            Console.WriteLine($"{e.Date:yyyy-MM-dd}  {e.Category,-13} ${e.Amount,8:0.00}  {e.Description}" +
                               (e.Notes is null ? "" : $"  ({e.Notes})"));
        }
        Console.WriteLine();
    }

    private static void ViewSummary()
    {
        if (_expenses.Count == 0)
        {
            Console.WriteLine("No expenses yet.\n");
            return;
        }

        var total = _expenses.Sum(e => e.Amount);
        var average = _expenses.Average(e => e.Amount);
        var highest = _expenses.MaxBy(e => e.Amount)!;

        Console.WriteLine($"Total spent:   ${total:0.00}");
        Console.WriteLine($"Average:       ${average:0.00}");
        Console.WriteLine($"Biggest:       ${highest.Amount:0.00} ({highest.Description})");
        Console.WriteLine();
        Console.WriteLine("By category:");

        // LINQ GroupBy: the classic "spend by category" breakdown.
        var byCategory = _expenses
            .GroupBy(e => e.Category)
            .Select(g => new { Category = g.Key, Total = g.Sum(e => e.Amount), Count = g.Count() })
            .OrderByDescending(x => x.Total);

        foreach (var group in byCategory)
        {
            Console.WriteLine($"  {group.Category,-13} ${group.Total,8:0.00}  ({group.Count} expense{(group.Count == 1 ? "" : "s")})");
        }
        Console.WriteLine();

        if (_budgets.Count > 0)
        {
            Console.WriteLine("Budget status (this month):");
            foreach (var budget in _budgets.OrderBy(b => b.Category))
            {
                var status = Budgets.GetStatus(_expenses, _budgets, budget.Category, DateTime.Today)!;
                var flag = status.IsOverBudget ? "  OVER BUDGET" : "";
                Console.WriteLine($"  {status.Category,-13} ${status.SpentThisMonth,8:0.00} / ${status.Limit,-8:0.00}{flag}");
            }
            Console.WriteLine();
        }
    }

    private static void FilterByCategory()
    {
        Console.Write("Category to filter by: ");
        var input = Console.ReadLine() ?? "";
        if (!Enum.TryParse<Category>(input, ignoreCase: true, out var category))
        {
            Console.WriteLine("Not a recognized category.\n");
            return;
        }

        var matches = _expenses.Where(e => e.Category == category).OrderByDescending(e => e.Date).ToList();
        if (matches.Count == 0)
        {
            Console.WriteLine("No matches.\n");
            return;
        }

        foreach (var e in matches)
            Console.WriteLine($"{e.Date:yyyy-MM-dd}  ${e.Amount,8:0.00}  {e.Description}");
        Console.WriteLine($"Subtotal: ${matches.Sum(e => e.Amount):0.00}\n");
    }

    private static void FilterByDateRange()
    {
        var start = ReadValue("From (yyyy-mm-dd): ", s =>
            DateTime.TryParse(s, out var d) ? (true, d) : (false, DateTime.MinValue));
        var end = ReadValue("To (yyyy-mm-dd): ", s =>
            DateTime.TryParse(s, out var d) ? (true, d) : (false, DateTime.MinValue));

        var matches = _expenses
            .Where(e => e.Date >= start && e.Date <= end)
            .OrderBy(e => e.Date)
            .ToList();

        if (matches.Count == 0)
        {
            Console.WriteLine("No matches.\n");
            return;
        }

        foreach (var e in matches)
            Console.WriteLine($"{e.Date:yyyy-MM-dd}  {e.Category,-13} ${e.Amount,8:0.00}  {e.Description}");
        Console.WriteLine($"Total: ${matches.Sum(e => e.Amount):0.00}\n");
    }

    private static void SearchExpenses()
    {
        Console.Write("Search text (matches description or notes): ");
        var term = Console.ReadLine() ?? "";
        if (string.IsNullOrWhiteSpace(term))
        {
            Console.WriteLine("Enter some search text.\n");
            return;
        }

        var matches = _expenses
            .Where(e => e.Description.Contains(term, StringComparison.OrdinalIgnoreCase)
                     || (e.Notes?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false))
            .OrderByDescending(e => e.Date)
            .ToList();

        if (matches.Count == 0)
        {
            Console.WriteLine("No matches.\n");
            return;
        }

        foreach (var e in matches)
            Console.WriteLine($"{e.Date:yyyy-MM-dd}  {e.Category,-13} ${e.Amount,8:0.00}  {e.Description}" +
                               (e.Notes is null ? "" : $"  ({e.Notes})"));
        Console.WriteLine($"Subtotal: ${matches.Sum(e => e.Amount):0.00}\n");
    }

    private static void EditExpense()
    {
        ViewAll();
        Console.Write("Description of the expense to edit (exact match): ");
        var description = Console.ReadLine() ?? "";
        var existing = _expenses.FirstOrDefault(e =>
            e.Description.Equals(description, StringComparison.OrdinalIgnoreCase));

        if (existing is null)
        {
            Console.WriteLine("Not found.\n");
            return;
        }

        var newAmount = ReadValue($"New amount (was ${existing.Amount:0.00}): $", s =>
            decimal.TryParse(s, out var d) && d > 0 ? (true, d) : (false, 0m));

        // "with" expression: records give you non-destructive updates for free.
        var updated = existing with { Amount = newAmount };
        Service.Validate(updated);

        var index = _expenses.IndexOf(existing);
        _expenses[index] = updated;
        Console.WriteLine("Updated.\n");
    }

    private static void DeleteExpense()
    {
        ViewAll();
        Console.Write("Description of the expense to delete (exact match): ");
        var description = Console.ReadLine() ?? "";
        var removed = _expenses.RemoveAll(e =>
            e.Description.Equals(description, StringComparison.OrdinalIgnoreCase));
        Console.WriteLine(removed > 0 ? $"Deleted {removed} expense(s).\n" : "Not found.\n");
    }

    private static void SetBudget()
    {
        Console.Write("Category to set a budget for: ");
        var input = Console.ReadLine() ?? "";
        if (!Enum.TryParse<Category>(input, ignoreCase: true, out var category))
        {
            Console.WriteLine("Not a recognized category.\n");
            return;
        }

        var limit = ReadValue("Monthly limit: $", s =>
            decimal.TryParse(s, out var d) && d > 0 ? (true, d) : (false, 0m));

        var budget = new Budget(category, limit);
        Budgets.Validate(budget); // throws InvalidBudgetException if something's off

        var existing = _budgets.FirstOrDefault(b => b.Category == category);
        if (existing is not null)
            _budgets[_budgets.IndexOf(existing)] = budget;
        else
            _budgets.Add(budget);

        Console.WriteLine($"Budget set: {category} -> ${limit:0.00}/month.\n");
    }

    private static void ViewMonthlyReport()
    {
        if (_expenses.Count == 0)
        {
            Console.WriteLine("No expenses yet.\n");
            return;
        }

        var byMonth = _expenses
            .GroupBy(e => new DateTime(e.Date.Year, e.Date.Month, 1))
            .Select(g => new { Month = g.Key, Total = g.Sum(e => e.Amount), Count = g.Count() })
            .OrderBy(x => x.Month);

        decimal? previous = null;
        foreach (var m in byMonth)
        {
            var trend = previous is null ? "" : m.Total > previous ? " (+)" : m.Total < previous ? " (-)" : " (=)";
            Console.WriteLine($"  {m.Month:yyyy-MM}   ${m.Total,10:0.00}  ({m.Count} expense{(m.Count == 1 ? "" : "s")}){trend}");
            previous = m.Total;
        }
        Console.WriteLine();
    }

    private static async Task ExportToCsvAsync()
    {
        if (_expenses.Count == 0)
        {
            Console.WriteLine("No expenses to export.\n");
            return;
        }

        var defaultPath = $"{_currentUser.Username}-expenses.csv";
        Console.Write($"Export file path (blank = {defaultPath}): ");
        var input = Console.ReadLine();
        var path = string.IsNullOrWhiteSpace(input) ? defaultPath : input;

        await CsvExport.ExportAsync(_expenses, path);
        Console.WriteLine($"Exported {_expenses.Count} expense(s) to {path}.\n");
    }
}
