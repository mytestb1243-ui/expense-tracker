using First_Console_shafi.Data;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace First_Console_shafi;

public class JsonFileBudgetStore : IBudgetStore
{
    private readonly string _filePath;
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public JsonFileBudgetStore(string filePath) => _filePath = filePath;

    public async Task<List<Budget>> LoadAsync()
    {
        if (!File.Exists(_filePath))
            return new List<Budget>();

        await using var stream = File.OpenRead(_filePath);
        var budgets = await JsonSerializer.DeserializeAsync<List<Budget>>(stream, Options);
        return budgets ?? new List<Budget>();
    }

    public async Task SaveAsync(List<Budget> budgets)
    {
        await using var stream = File.Create(_filePath);
        await JsonSerializer.SerializeAsync(stream, budgets, Options);
    }
}
