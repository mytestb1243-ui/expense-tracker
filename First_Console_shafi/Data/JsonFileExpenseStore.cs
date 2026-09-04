using First_Console_shafi.Data;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace First_Console_shafi;

public class JsonFileExpenseStore : IExpenseStore
{
    private readonly string _filePath;
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() } // store "Entertainment", not 2
    };

    public JsonFileExpenseStore(string filePath) => _filePath = filePath;

    public async Task<List<Expense>> LoadAsync()
    {
        if (!File.Exists(_filePath))
            return new List<Expense>();

        await using var stream = File.OpenRead(_filePath);
        var expenses = await JsonSerializer.DeserializeAsync<List<Expense>>(stream, Options);
        return expenses ?? new List<Expense>();
    }

    public async Task SaveAsync(List<Expense> expenses)
    {
        await using var stream = File.Create(_filePath);
        await JsonSerializer.SerializeAsync(stream, expenses, Options);
    }
}