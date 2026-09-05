namespace First_Console_shafi;

// Where a category's spending stands against its monthly budget, if one is set.
public record BudgetStatus(Category Category, decimal Limit, decimal SpentThisMonth, bool IsOverBudget);

// Budget business rules, kept UI-free and testable like ExpenseService.
public class BudgetService
{
    public void Validate(Budget budget)
    {
        ArgumentNullException.ThrowIfNull(budget);

        if (budget.MonthlyLimit <= 0)
            throw new InvalidBudgetException("Monthly limit must be greater than zero.");
    }

    // Returns null when no budget is set for the category.
    public BudgetStatus? GetStatus(IEnumerable<Expense> expenses, IEnumerable<Budget> budgets, Category category, DateTime referenceDate)
    {
        var budget = budgets.FirstOrDefault(b => b.Category == category);
        if (budget is null) return null;

        var spent = expenses
            .Where(e => e.Category == category && e.Date.Year == referenceDate.Year && e.Date.Month == referenceDate.Month)
            .Sum(e => e.Amount);

        return new BudgetStatus(category, budget.MonthlyLimit, spent, spent > budget.MonthlyLimit);
    }
}
