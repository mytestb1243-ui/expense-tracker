using First_Console_shafi.Data;
using System.Text.Json;

namespace First_Console_shafi;

public class JsonFileUserStore : IUserStore
{
    private readonly string _filePath;
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    public JsonFileUserStore(string filePath) => _filePath = filePath;

    public async Task<List<User>> LoadAsync()
    {
        if (!File.Exists(_filePath))
            return new List<User>();

        await using var stream = File.OpenRead(_filePath);
        var users = await JsonSerializer.DeserializeAsync<List<User>>(stream, Options);
        return users ?? new List<User>();
    }

    public async Task SaveAsync(List<User> users)
    {
        await using var stream = File.Create(_filePath);
        await JsonSerializer.SerializeAsync(stream, users, Options);
    }
}
