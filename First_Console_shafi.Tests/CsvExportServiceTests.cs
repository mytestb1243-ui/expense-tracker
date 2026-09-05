using First_Console_shafi;

namespace First_Console_shafi.Tests;

public class CsvExportServiceTests
{
    private readonly CsvExportService _service = new();

    [Fact]
    public void ToCsv_includes_a_header_row()
    {
        var csv = _service.ToCsv([]);

        Assert.StartsWith("Id,Description,Amount,Category,Date,Notes", csv);
    }

    [Fact]
    public void ToCsv_quotes_a_description_containing_a_comma()
    {
        var expense = new Expense(Guid.NewGuid(), "Coffee, large", 4.50m, Category.Food, new DateTime(2026, 3, 1));

        var csv = _service.ToCsv([expense]);

        Assert.Contains("\"Coffee, large\"", csv);
    }

    [Fact]
    public void ToCsv_doubles_embedded_quotes()
    {
        var result = CsvExportService.EscapeField("She said \"hi\"");

        Assert.Equal("\"She said \"\"hi\"\"\"", result);
    }

    [Fact]
    public void ToCsv_formats_amount_with_invariant_culture_and_two_decimals()
    {
        var expense = new Expense(Guid.NewGuid(), "Snack", 3m, Category.Food, new DateTime(2026, 3, 1));

        var csv = _service.ToCsv([expense]);

        Assert.Contains(",3.00,", csv);
    }
}
