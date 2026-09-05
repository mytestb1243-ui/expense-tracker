namespace First_Console_shafi;

// A local account: password is never stored, only a PBKDF2 hash + its salt.
public record User(
    Guid Id,
    string Username,
    string PasswordHash,
    string PasswordSalt,
    int Iterations,
    DateTime CreatedAt
);
