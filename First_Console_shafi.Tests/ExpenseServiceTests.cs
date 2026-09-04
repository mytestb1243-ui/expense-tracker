using First_Console_shafi;

namespace First_Console_shafi.Tests;

public class ExpenseServiceTests
{
    private readonly ExpenseService _service = new();

    private static Expense Valid(
        string description = "Coffee",
        decimal amount = 3.50m,
        DateTime? date = null) => new(
            Guid.NewGuid(),
            description,
            amount,
            Category.Food,
            date ?? DateTime.Today);

    [Fact]
    public void Validate_accepts_a_well_formed_expense()
    {
        var ex = Record.Exception(() => _service.Validate(Valid()));

        Assert.Null(ex);
    }

    [Fact]
    public void Validate_accepts_an_expense_dated_today()
    {
        var ex = Record.Exception(() => _service.Validate(Valid(date: DateTime.Today)));

        Assert.Null(ex);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t")]
    public void Validate_rejects_a_blank_description(string description)
    {
        var ex = Assert.Throws<InvalidExpenseException>(
            () => _service.Validate(Valid(description: description)));

        Assert.Equal("Description can't be empty.", ex.Message);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-0.01)]
    [InlineData(-100)]
    public void Validate_rejects_a_non_positive_amount(decimal amount)
    {
        var ex = Assert.Throws<InvalidExpenseException>(
            () => _service.Validate(Valid(amount: amount)));

        Assert.Equal("Amount must be greater than zero.", ex.Message);
    }

    [Fact]
    public void Validate_rejects_a_future_date()
    {
        var tomorrow = DateTime.Today.AddDays(1);

        var ex = Assert.Throws<InvalidExpenseException>(
            () => _service.Validate(Valid(date: tomorrow)));

        Assert.Equal("Date can't be in the future.", ex.Message);
    }

    [Fact]
    public void Validate_ignores_the_time_component_of_todays_date()
    {
        // "Today at 23:00" is still today, even though it is > DateTime.Today.
        var laterToday = DateTime.Today.AddHours(23);

        var ex = Record.Exception(() => _service.Validate(Valid(date: laterToday)));

        Assert.Null(ex);
    }

    [Fact]
    public void Validate_throws_ArgumentNullException_for_a_null_expense()
    {
        Assert.Throws<ArgumentNullException>(() => _service.Validate(null!));
    }

    [Fact]
    public void Validate_reports_the_description_rule_before_the_amount_rule()
    {
        var bad = new Expense(Guid.NewGuid(), "", -5m, Category.Food, DateTime.Today);

        var ex = Assert.Throws<InvalidExpenseException>(() => _service.Validate(bad));

        Assert.Equal("Description can't be empty.", ex.Message);
    }

    [Fact]
    public void TryValidate_returns_true_and_no_error_for_a_valid_expense()
    {
        var ok = _service.TryValidate(Valid(), out var error);

        Assert.True(ok);
        Assert.Null(error);
    }

    [Fact]
    public void TryValidate_returns_false_and_the_message_for_an_invalid_expense()
    {
        var ok = _service.TryValidate(Valid(amount: 0m), out var error);

        Assert.False(ok);
        Assert.Equal("Amount must be greater than zero.", error);
    }
}
