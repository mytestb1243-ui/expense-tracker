namespace First_Console_shafi.Data;

// Abstraction over "how budgets get persisted" — mirrors IExpenseStore.
public interface IBudgetStore
{
    Task<List<Budget>> LoadAsync();
    Task SaveAsync(List<Budget> budgets);
}
