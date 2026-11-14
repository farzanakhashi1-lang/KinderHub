namespace KinderHub.Web.Components.Shared;

// One data series of a chart. ColorSlot 1, 2, 3... picks a fixed color (never by rank).
public record ChartSeries(string Name, IReadOnlyList<double> Values, int ColorSlot);
