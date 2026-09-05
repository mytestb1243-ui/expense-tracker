namespace First_Console_shafi.Data;

// Abstraction over "how accounts get persisted" — mirrors IExpenseStore.
public interface IUserStore
{
    Task<List<User>> LoadAsync();
    Task SaveAsync(List<User> users);
}
