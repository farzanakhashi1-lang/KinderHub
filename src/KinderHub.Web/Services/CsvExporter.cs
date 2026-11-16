using System.Text;
using KinderHub.Application.DTOs;

namespace KinderHub.Web.Services;

// Builds CSV files (open in Excel / Google Sheets)
public static class CsvExporter
{
    public static byte[] Attendance(IEnumerable<AttendanceDto> rows)
    {
        var sb = new StringBuilder();
        sb.AppendLine("sep=,");
        sb.AppendLine("Date,Child,Status,Check-in,Check-out,Picked up by,Note");

        foreach (var r in rows)
        {
            sb.AppendLine(string.Join(",",
                Cell(r.Date.ToString("yyyy-MM-dd")),
                Cell(r.ChildName),
                Cell(r.Status.ToString()),
                Cell(r.CheckInTime?.ToString("HH:mm")),
                Cell(r.CheckOutTime?.ToString("HH:mm")),
                Cell(r.PickedUpBy),
                Cell(r.Note)));
        }

        // UTF-8 with BOM so Excel shows Kazakh/Russian letters correctly
        return Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
    }

    // Puts the value in quotes and blocks "CSV injection":
    // a cell starting with = + - @ could run a formula when opened in Excel.
    private static string Cell(string? value)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;
        if ("=+-@".Contains(value[0])) value = "'" + value;
        return $"\"{value.Replace("\"", "\"\"")}\"";
    }
}
