using First_Console_shafi;

namespace First_Console_shafi.Tests;

public class JsonFileBudgetStoreTests : IDisposable
{
    private readonly string _path = Path.Combine(
        Path.GetTempPath(), $"budgets-test-{Guid.NewGuid():N}.json");

    public void Dispose()
    {
        if (File.Exists(_path))
            File.Delete(_path);
    }

    [Fact]
    public async Task LoadAsync_returns_empty_list_when_file_is_missing()
    {
        var store = new JsonFileBudgetStore(_path);

        var budgets = await store.LoadAsync();

        Assert.Empty(budgets);
    }

    [Fact]
    public async Task SaveAsync_then_LoadAsync_round_trips_the_data()
    {
        var store = new JsonFileBudgetStore(_path);
        var original = new List<Budget>
        {
            new(Category.Food, 200m),
            new(Category.Transport, 50m),
        };

        await store.SaveAsync(original);
        var loaded = await store.LoadAsync();

        Assert.Equal(original, loaded);
    }

    [Fact]
    public async Task SaveAsync_writes_the_category_as_a_string()
    {
        var store = new JsonFileBudgetStore(_path);

        await store.SaveAsync(new List<Budget> { new(Category.Shopping, 100m) });

        var json = await File.ReadAllTextAsync(_path);
        Assert.Contains("\"Shopping\"", json);
    }
}
