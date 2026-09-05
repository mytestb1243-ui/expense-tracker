using First_Console_shafi;

namespace First_Console_shafi.Tests;

public class AuthenticationExceptionTests
{
    [Fact]
    public void Carries_the_message_it_was_given()
    {
        var ex = new AuthenticationException("Invalid username or password.");

        Assert.Equal("Invalid username or password.", ex.Message);
        Assert.IsAssignableFrom<Exception>(ex);
    }
}
