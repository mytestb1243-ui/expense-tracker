using First_Console_shafi;

namespace First_Console_shafi.Tests;

public class InvalidExpenseExceptionTests
{
    [Fact]
    public void Carries_the_message_it_was_given()
    {
        var ex = new InvalidExpenseException("Amount must be greater than zero.");

        Assert.Equal("Amount must be greater than zero.", ex.Message);
        Assert.IsAssignableFrom<Exception>(ex);
    }
}
