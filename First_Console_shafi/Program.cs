using First_Console_shafi.Data;
using System.Globalization;

namespace First_Console_shafi;

class Program
{
    private static readonly IExpenseStore Store = new JsonFileExpenseStore("expenses.json");
    private static readonly ExpenseService Service = new();
    private static List<Expense> _expenses = new();

    static async Task Main()
    {
        // async in action: loading from disk doesn't block the thread while it waits on I/O.
        _expenses = await Store.LoadAsync();
        Console.WriteLine($"Loaded {_expenses.Count} expense(s) from disk.\n");

        var running = true;
        while (running)
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
                    case "6": EditExpense(); break;
                    case "7": DeleteExpense(); break;
                    case "8": running = false; break;
                    default: Console.WriteLine("Unknown option.\n"); break;
                }
            }
            catch (InvalidExpenseException ex)
            {
                // Custom exception, caught right where it's meaningful to the user.
                Console.WriteLine($"  Couldn't save that expense: {ex.Message}\n");
            }
        }

        await Store.SaveAsync(_expenses);
        Console.WriteLine("Saved. Bye!");
    }

    private static void PrintMenu()
    {
        Console.WriteLine("==== Expense Tracker ====");
        Console.WriteLine("1. Add expense");
        Console.WriteLine("2. View all expenses");
        Console.WriteLine("3. View summary");
        Console.WriteLine("4. Filter by category");
        Console.WriteLine("5. Filter by date range");
        Console.WriteLine("6. Edit an expense");
        Console.WriteLine("7. Delete an expense");
        Console.WriteLine("8. Exit");
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
        Console.WriteLine("Added.\n");
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
}
