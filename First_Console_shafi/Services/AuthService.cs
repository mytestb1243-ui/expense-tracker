using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace First_Console_shafi;

// Local account registration/login. UI-free and testable, like ExpenseService.
// Passwords are never stored — only a PBKDF2 hash + a random per-user salt.
public partial class AuthService
{
    private const int SaltSizeBytes = 16;
    private const int HashSizeBytes = 32;
    private const int DefaultIterations = 100_000;

    [GeneratedRegex("^[A-Za-z0-9_-]{3,20}$")]
    private static partial Regex UsernamePattern();

    public User Register(List<User> users, string username, string password)
    {
        ArgumentNullException.ThrowIfNull(users);
        username = (username ?? string.Empty).Trim();

        if (!UsernamePattern().IsMatch(username))
            throw new AuthenticationException("Username must be 3-20 characters: letters, digits, '_' or '-' only.");
        if (string.IsNullOrEmpty(password) || password.Length < 6)
            throw new AuthenticationException("Password must be at least 6 characters.");
        if (users.Any(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase)))
            throw new AuthenticationException("Username already exists.");

        var (hash, salt) = CreateHash(password, DefaultIterations);
        return new User(Guid.NewGuid(), username, hash, salt, DefaultIterations, DateTime.UtcNow);
    }

    public User Login(List<User> users, string username, string password)
    {
        ArgumentNullException.ThrowIfNull(users);
        username = (username ?? string.Empty).Trim();

        var user = users.FirstOrDefault(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
        // Same message whether the username is unknown or the password is wrong,
        // so a failed login never reveals which usernames actually exist.
        if (user is null || !VerifyHash(password, user.PasswordHash, user.PasswordSalt, user.Iterations))
            throw new AuthenticationException("Invalid username or password.");

        return user;
    }

    private static (string Hash, string Salt) CreateHash(string password, int iterations)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSizeBytes);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, HashSizeBytes);
        return (Convert.ToBase64String(hash), Convert.ToBase64String(salt));
    }

    private static bool VerifyHash(string password, string hash, string salt, int iterations)
    {
        var saltBytes = Convert.FromBase64String(salt);
        var expected = Convert.FromBase64String(hash);
        var actual = Rfc2898DeriveBytes.Pbkdf2(password, saltBytes, iterations, HashAlgorithmName.SHA256, expected.Length);
        return CryptographicOperations.FixedTimeEquals(expected, actual);
    }
}
