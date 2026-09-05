using First_Console_shafi;

namespace First_Console_shafi.Tests;

public class JsonFileUserStoreTests : IDisposable
{
    private readonly string _path = Path.Combine(
        Path.GetTempPath(), $"users-test-{Guid.NewGuid():N}.json");

    public void Dispose()
    {
        if (File.Exists(_path))
            File.Delete(_path);
    }

    [Fact]
    public async Task LoadAsync_returns_empty_list_when_file_is_missing()
    {
        var store = new JsonFileUserStore(_path);

        var users = await store.LoadAsync();

        Assert.Empty(users);
    }

    [Fact]
    public async Task SaveAsync_then_LoadAsync_round_trips_the_data()
    {
        var store = new JsonFileUserStore(_path);
        var original = new List<User>
        {
            new(Guid.NewGuid(), "alice", "hash", "salt", 100_000, new DateTime(2026, 3, 1)),
        };

        await store.SaveAsync(original);
        var loaded = await store.LoadAsync();

        Assert.Equal(original, loaded);
    }
}
