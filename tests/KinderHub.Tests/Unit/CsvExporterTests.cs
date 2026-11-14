using System.Text;
using KinderHub.Application.DTOs;
using KinderHub.Domain.Enums;
using KinderHub.Web.Services;
using Xunit;

namespace KinderHub.Tests.Unit;

public class CsvExporterTests
{
    private static string Export(params AttendanceDto[] rows)
    {
        var bytes = CsvExporter.Attendance(rows);
        return Encoding.UTF8.GetString(bytes).TrimStart('﻿');
    }

    private static AttendanceDto Row(string childName, string? note = null) =>
        new(1, 1, childName, new DateOnly(2026, 11, 13), AttendanceStatus.Present,
            new TimeOnly(8, 30), new TimeOnly(17, 15), "Mother", note);

    [Fact]
    public void Starts_with_header_and_has_one_line_per_record()
    {
        var lines = Export(Row("Aruzhan"), Row("Alikhan")).Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

        Assert.Equal("Date,Child,Status,Check-in,Check-out,Picked up by,Note", lines[0]);
        Assert.Equal(3, lines.Length);
        Assert.Contains("\"Aruzhan\"", lines[1]);
        Assert.Contains("\"08:30\"", lines[1]);
    }

    [Fact]
    public void File_starts_with_utf8_bom_for_excel()
    {
        var bytes = CsvExporter.Attendance(new[] { Row("Aruzhan") });
        Assert.Equal(new byte[] { 0xEF, 0xBB, 0xBF }, bytes.Take(3).ToArray());
    }

    [Fact]
    public void Quotes_inside_values_are_escaped()
    {
        var csv = Export(Row("Ann \"Nickname\" Lee"));
        Assert.Contains("\"Ann \"\"Nickname\"\" Lee\"", csv);
    }

    [Theory]
    [InlineData("=HYPERLINK(\"http://evil\")")]
    [InlineData("+1+1")]
    [InlineData("-2")]
    [InlineData("@SUM(A1)")]
    public void Formulas_are_neutralised_to_prevent_csv_injection(string dangerous)
    {
        var csv = Export(Row("Child", note: dangerous));
        Assert.Contains("\"'" + dangerous[0], csv);
    }
}
