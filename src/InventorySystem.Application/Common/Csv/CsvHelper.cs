using System.Globalization;
using System.Text;

namespace InventorySystem.Application.Common.Csv;

/// <summary>
/// Minimal CSV writer (no external library): UTF-8 with BOM so Excel opens
/// Arabic text correctly, RFC 4180 field escaping.
/// </summary>
public static class CsvHelper
{
    public static string Escape(string? value)
    {
        if (value is null)
            return string.Empty;
        if (value.Contains('"') || value.Contains(',') || value.Contains('\n') || value.Contains('\r'))
            return "\"" + value.Replace("\"", "\"\"") + "\"";
        return value;
    }

    public static string Build(IEnumerable<string> headers, IEnumerable<IEnumerable<string?>> rows)
    {
        var sb = new StringBuilder();
        sb.AppendLine(string.Join(",", headers.Select(Escape)));
        foreach (var row in rows)
            sb.AppendLine(string.Join(",", row.Select(Escape)));
        return sb.ToString();
    }

    public static string FormatDecimal(decimal value) =>
        value.ToString("0.00", CultureInfo.InvariantCulture);

    public static string FormatDate(DateTime value) =>
        value.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
}
