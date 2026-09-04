namespace First_Console_shafi;

// Business rules that don't belong in the console UI.
// Kept public and free of Console calls so it can be unit-tested directly
// and reused if the app ever grows a second front end.
public class ExpenseService
{
    // Throws InvalidExpenseException describing the first rule that fails.
    public void Validate(Expense expense)
    {
        ArgumentNullException.ThrowIfNull(expense);

        if (string.IsNullOrWhiteSpace(expense.Description))
            throw new InvalidExpenseException("Description can't be empty.");
        if (expense.Amount <= 0)
            throw new InvalidExpenseException("Amount must be greater than zero.");
        if (expense.Date.Date > DateTime.Today)
            throw new InvalidExpenseException("Date can't be in the future.");
    }

    // Non-throwing variant for callers that just want a yes/no plus the reason.
    public bool TryValidate(Expense expense, out string? error)
    {
        try
        {
            Validate(expense);
            error = null;
            return true;
        }
        catch (InvalidExpenseException ex)
        {
            error = ex.Message;
            return false;
        }
    }
}
