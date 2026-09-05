using First_Console_shafi;

namespace First_Console_shafi.Tests;

public class InvalidBudgetExceptionTests
{
    [Fact]
    public void Carries_the_message_it_was_given()
    {
        var ex = new InvalidBudgetException("Monthly limit must be greater than zero.");

        Assert.Equal("Monthly limit must be greater than zero.", ex.Message);
        Assert.IsAssignableFrom<Exception>(ex);
    }
}
