using First_Console_shafi;

namespace First_Console_shafi.Tests;

public class BudgetServiceTests
{
    private readonly BudgetService _service = new();

    private static Expense Expense(decimal amount, Category category, DateTime date) =>
        new(Guid.NewGuid(), "Item", amount, category, date);

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void Validate_rejects_a_non_positive_monthly_limit(decimal limit)
    {
        var ex = Assert.Throws<InvalidBudgetException>(() => _service.Validate(new Budget(Category.Food, limit)));

        Assert.Equal("Monthly limit must be greater than zero.", ex.Message);
    }

    [Fact]
    public void GetStatus_returns_null_when_no_budget_is_set_for_the_category()
    {
        var status = _service.GetStatus([], [], Category.Food, DateTime.Today);

        Assert.Null(status);
    }

    [Fact]
    public void GetStatus_reports_under_budget_when_spending_is_below_the_limit()
    {
        var today = new DateTime(2026, 3, 15);
        var expenses = new List<Expense> { Expense(20m, Category.Food, today) };
        var budgets = new List<Budget> { new(Category.Food, 50m) };

        var status = _service.GetStatus(expenses, budgets, Category.Food, today)!;

        Assert.Equal(20m, status.SpentThisMonth);
        Assert.False(status.IsOverBudget);
    }

    [Fact]
    public void GetStatus_reports_over_budget_when_spending_exceeds_the_limit()
    {
        var today = new DateTime(2026, 3, 15);
        var expenses = new List<Expense> { Expense(60m, Category.Food, today) };
        var budgets = new List<Budget> { new(Category.Food, 50m) };

        var status = _service.GetStatus(expenses, budgets, Category.Food, today)!;

        Assert.True(status.IsOverBudget);
    }

    [Fact]
    public void GetStatus_only_counts_expenses_in_the_reference_month()
    {
        var referenceDate = new DateTime(2026, 3, 15);
        var expenses = new List<Expense>
        {
            Expense(60m, Category.Food, new DateTime(2026, 2, 20)), // previous month, excluded
            Expense(10m, Category.Food, referenceDate),
        };
        var budgets = new List<Budget> { new(Category.Food, 50m) };

        var status = _service.GetStatus(expenses, budgets, Category.Food, referenceDate)!;

        Assert.Equal(10m, status.SpentThisMonth);
        Assert.False(status.IsOverBudget);
    }
}
