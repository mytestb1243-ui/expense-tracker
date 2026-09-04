namespace First_Console_shafi.Data;

// Abstraction over "how expenses get persisted."
// In Phase 3 of your roadmap you'll write an EF Core-backed implementation of this
// same interface without touching any of the menu or LINQ code in Program.cs —
// that's the whole point of coding against an interface instead of a concrete class.
public interface IExpenseStore
{
    Task<List<Expense>> LoadAsync();
    Task SaveAsync(List<Expense> expenses);
} 