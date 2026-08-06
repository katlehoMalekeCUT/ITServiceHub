using System.Text;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.Extensions.Options;
using ServiceHub_IT.Models;

namespace ServiceHub_IT.Services;

public class ReportService
{
    private readonly HttpClient _httpClient;
    private readonly SupabaseSettings _settings;
    private readonly ILogger<ReportService> _logger;

    public ReportService(HttpClient httpClient, IOptions<SupabaseSettings> options, ILogger<ReportService> logger)
    {
        _httpClient = httpClient;
        _settings = options.Value;
        _logger = logger;
    }

    public Task<MemoryStream> GenerateExcelAsync(string reportType, IEnumerable<Dictionary<string, object?>> rows, CancellationToken cancellationToken = default)
    {
        var stream = new MemoryStream();
        var document = SpreadsheetDocument.Create(stream, SpreadsheetDocumentType.Workbook, true);

        var workbookPart = document.AddWorkbookPart();
        workbookPart.Workbook = new Workbook();

        var worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
        worksheetPart.Worksheet = new Worksheet(new SheetData());

        var sheets = workbookPart.Workbook.AppendChild(new Sheets());
        sheets.AppendChild(new Sheet { Id = workbookPart.GetIdOfPart(worksheetPart), SheetId = 1, Name = reportType });

        var sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();
        if (sheetData is null)
        {
            sheetData = new SheetData();
            worksheetPart.Worksheet.AppendChild(sheetData);
        }

        var headerRow = new Row();
        var columns = rows.FirstOrDefault()?.Keys.ToList() ?? new List<string>();
        foreach (var column in columns)
        {
            headerRow.AppendChild(new Cell(new InlineString(new Text(column))) { DataType = CellValues.InlineString });
        }
        sheetData.AppendChild(headerRow);

        foreach (var row in rows)
        {
            var dataRow = new Row();
            foreach (var column in columns)
            {
                var cellValue = row.TryGetValue(column, out var value) ? value?.ToString() ?? string.Empty : string.Empty;
                dataRow.AppendChild(new Cell(new InlineString(new Text(cellValue))) { DataType = CellValues.InlineString });
            }
            sheetData.AppendChild(dataRow);
        }

        workbookPart.Workbook.Save();
        document.Dispose();
        stream.Position = 0;
        return Task.FromResult(stream);
    }

    public string BuildCsv(IEnumerable<Dictionary<string, object?>> rows)
    {
        if (rows is null || !rows.Any())
        {
            return string.Empty;
        }

        var columns = rows.First().Keys.ToList();
        var builder = new StringBuilder();
        builder.AppendLine(string.Join(",", columns.Select(QuoteCsv)));
        foreach (var row in rows)
        {
            builder.AppendLine(string.Join(",", columns.Select(c => QuoteCsv(row.TryGetValue(c, out var value) ? value?.ToString() ?? string.Empty : string.Empty))));
        }
        return builder.ToString();
    }

    private static string QuoteCsv(string value) => value.Contains(',') || value.Contains('"') || value.Contains('\n') ? $"\"{value.Replace("\"", "\"")}" : value;
}
