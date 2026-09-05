using First_Console_shafi;

namespace First_Console_shafi.Tests;

public class AuthServiceTests
{
    private readonly AuthService _service = new();

    [Fact]
    public void Register_creates_a_user_whose_password_hash_differs_from_the_plaintext_password()
    {
        var user = _service.Register(new List<User>(), "alice", "hunter22");

        Assert.Equal("alice", user.Username);
        Assert.NotEqual("hunter22", user.PasswordHash);
        Assert.NotEmpty(user.PasswordSalt);
    }

    [Fact]
    public void Register_rejects_a_duplicate_username_case_insensitively()
    {
        var existing = _service.Register(new List<User>(), "alice", "hunter22");

        var ex = Assert.Throws<AuthenticationException>(
            () => _service.Register([existing], "ALICE", "another-password"));

        Assert.Equal("Username already exists.", ex.Message);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("ab")]
    [InlineData("this-username-is-way-too-long")]
    [InlineData("bad name")]
    public void Register_rejects_an_invalid_username(string username)
    {
        Assert.Throws<AuthenticationException>(() => _service.Register(new List<User>(), username, "hunter22"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("short")]
    public void Register_rejects_a_password_shorter_than_6_characters(string password)
    {
        var ex = Assert.Throws<AuthenticationException>(() => _service.Register(new List<User>(), "alice", password));

        Assert.Equal("Password must be at least 6 characters.", ex.Message);
    }

    [Fact]
    public void Login_returns_the_user_for_correct_credentials()
    {
        var registered = _service.Register(new List<User>(), "alice", "hunter22");

        var loggedIn = _service.Login([registered], "alice", "hunter22");

        Assert.Equal(registered, loggedIn);
    }

    [Fact]
    public void Login_is_case_insensitive_on_username()
    {
        var registered = _service.Register(new List<User>(), "alice", "hunter22");

        var loggedIn = _service.Login([registered], "ALICE", "hunter22");

        Assert.Equal(registered, loggedIn);
    }

    [Fact]
    public void Login_throws_AuthenticationException_for_an_unknown_username()
    {
        var ex = Assert.Throws<AuthenticationException>(() => _service.Login(new List<User>(), "ghost", "hunter22"));

        Assert.Equal("Invalid username or password.", ex.Message);
    }

    [Fact]
    public void Login_throws_the_same_message_for_a_wrong_password()
    {
        var registered = _service.Register(new List<User>(), "alice", "hunter22");

        var ex = Assert.Throws<AuthenticationException>(() => _service.Login([registered], "alice", "wrong-password"));

        Assert.Equal("Invalid username or password.", ex.Message);
    }
}
