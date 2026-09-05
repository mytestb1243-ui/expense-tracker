using System.Globalization;
using System.Text;

namespace First_Console_shafi;

// Turns expenses into RFC 4046-ish CSV. ToCsv has no file I/O, so it's directly unit-testable.
public class CsvExportService
{
    public string ToCsv(IEnumerable<Expense> expenses)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Id,Description,Amount,Category,Date,Notes");

        foreach (var e in expenses)
        {
            sb.AppendLine(string.Join(",",
                e.Id,
                EscapeField(e.Description),
                e.Amount.ToString("0.00", CultureInfo.InvariantCulture),
                e.Category,
                e.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                EscapeField(e.Notes)));
        }

        return sb.ToString();
    }

    public async Task ExportAsync(IEnumerable<Expense> expenses, string filePath)
    {
        await File.WriteAllTextAsync(filePath, ToCsv(expenses));
    }

    // RFC 4180: quote a field if it contains a comma, quote, or newline; double any embedded quotes.
    internal static string EscapeField(string? field)
    {
        if (string.IsNullOrEmpty(field)) return string.Empty;

        var needsQuoting = field.IndexOfAny([',', '"', '\n', '\r']) >= 0;
        if (!needsQuoting) return field;

        return $"\"{field.Replace("\"", "\"\"")}\"";
    }
}
