using First_Console_shafi;

namespace First_Console_shafi.Tests;

public class JsonFileExpenseStoreTests : IDisposable
{
    private readonly string _path = Path.Combine(
        Path.GetTempPath(), $"expenses-test-{Guid.NewGuid():N}.json");

    public void Dispose()
    {
        if (File.Exists(_path))
            File.Delete(_path);
    }

    [Fact]
    public async Task LoadAsync_returns_empty_list_when_file_is_missing()
    {
        var store = new JsonFileExpenseStore(_path);

        var expenses = await store.LoadAsync();

        Assert.Empty(expenses);
    }

    [Fact]
    public async Task SaveAsync_then_LoadAsync_round_trips_the_data()
    {
        var store = new JsonFileExpenseStore(_path);
        var original = new List<Expense>
        {
            new(Guid.NewGuid(), "Bus fare", 2.50m, Category.Transport, new DateTime(2026, 2, 1)),
            new(Guid.NewGuid(), "Cinema", 18m, Category.Entertainment, new DateTime(2026, 2, 3), "date night"),
        };

        await store.SaveAsync(original);
        var loaded = await store.LoadAsync();

        Assert.Equal(original, loaded);
    }

    [Fact]
    public async Task SaveAsync_writes_the_category_as_a_string()
    {
        var store = new JsonFileExpenseStore(_path);

        await store.SaveAsync(new List<Expense>
        {
            new(Guid.NewGuid(), "Groceries", 40m, Category.Shopping, new DateTime(2026, 2, 5)),
        });

        var json = await File.ReadAllTextAsync(_path);
        Assert.Contains("\"Shopping\"", json);
        Assert.DoesNotContain("\"Category\": 4", json);
    }
}
