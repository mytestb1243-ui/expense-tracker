using First_Console_shafi;

namespace First_Console_shafi.Tests;

public class ExpenseTests
{
    private static Expense Sample(decimal amount = 10m) => new(
        Guid.Parse("11111111-1111-1111-1111-111111111111"),
        "Coffee",
        amount,
        Category.Food,
        new DateTime(2026, 1, 15),
        Notes: null);

    [Fact]
    public void Records_with_the_same_values_are_equal()
    {
        Assert.Equal(Sample(), Sample());
    }

    [Fact]
    public void With_expression_changes_one_field_and_leaves_the_rest()
    {
        var original = Sample(10m);

        var updated = original with { Amount = 25m };

        Assert.Equal(25m, updated.Amount);
        Assert.Equal(original.Id, updated.Id);
        Assert.Equal(original.Description, updated.Description);
        Assert.NotEqual(original, updated);
    }

    [Fact]
    public void Notes_defaults_to_null()
    {
        var expense = new Expense(Guid.NewGuid(), "Lunch", 12m, Category.Food, DateTime.Today);

        Assert.Null(expense.Notes);
    }
}
