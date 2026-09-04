namespace First_Console_shafi;

// A record: C#'s go-to type for data that's mostly about its values.
// Equality, ToString(), and a copy-with-changes ("with" expression) all come for free.
public record Expense(
    Guid Id,
    string Description,
    decimal Amount,
    Category Category,
    DateTime Date,
    string? Notes = null // nullable reference type: this field is genuinely optional
);