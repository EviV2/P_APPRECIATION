// elias veya
// affichage scottplot

using System;
using System.Collections.Generic;
using System.Linq;
using ScottPlot;
using ScottPlot.Plottables;
using BanStatsApp.Models;

namespace BanStatsApp.Services;

public class PlotService
{
    private Marker? _selectedMarker;
    private Text? _selectedAnnotation;

    /// <summary>
    /// regle le style visuel et les axes du graphique
    /// </summary>
    public void ConfigurePlotStyle(Plot plot)
    {
        plot.FigureBackground.Color = Color.FromHex("#FFFFFF");
        plot.DataBackground.Color = Color.FromHex("#FFFFFF");

        plot.Legend.IsVisible = false;
        plot.Benchmark.IsVisible = false;

        plot.Grid.MajorLineColor = Color.FromHex("#F1F5F9");
        plot.Grid.MajorLineWidth = 1;

        plot.Axes.Left.Label.Text = "Percentage (%)";
        plot.Axes.Left.Label.Bold = true;
        plot.Axes.Left.Label.FontSize = 13;
        plot.Axes.Left.Label.ForeColor = Color.FromHex("#334155");
        plot.Axes.Left.MinimumSize = 55;

        plot.Axes.Bottom.Label.Text = "Patch / Date";
        plot.Axes.Bottom.Label.Bold = true;
        plot.Axes.Bottom.Label.FontSize = 13;
        plot.Axes.Bottom.Label.ForeColor = Color.FromHex("#334155");
        plot.Axes.Bottom.MinimumSize = 65;
        plot.Axes.Bottom.TickLabelStyle.FontSize = 11;
    }

    /// <summary>
    /// trace les series demandees sur le graphique
    /// </summary>
    public void UpdateSeries(
        Plot plot,
        IReadOnlyList<BanRateRecord> records,
        bool showBanRate,
        bool showWinRate,
        bool showPickRate)
    {
        plot.Clear();
        _selectedMarker = null;
        _selectedAnnotation = null;

        if (records == null || records.Count == 0)
        {
            return;
        }

        double[] xs = records.Select(r => (double)r.Index).ToArray();

        // courbe banrate
        if (showBanRate)
        {
            var scatter = plot.Add.Scatter(xs, records.Select(r => r.BanRate).ToArray());
            scatter.Color = Color.FromHex("#E0247C");
            scatter.LineWidth = 2.4f;
            scatter.MarkerSize = 0f;
            scatter.LegendText = "Banrate";
        }

        // courbe winrate
        if (showWinRate)
        {
            var scatter = plot.Add.Scatter(xs, records.Select(r => r.WinRate).ToArray());
            scatter.Color = Color.FromHex("#1E293B");
            scatter.LineWidth = 2.0f;
            scatter.MarkerSize = 0f;
            scatter.LegendText = "Winrate";
        }

        // courbe pickrate
        if (showPickRate)
        {
            var scatter = plot.Add.Scatter(xs, records.Select(r => r.PickRate).ToArray());
            scatter.Color = Color.FromHex("#EF4444");
            scatter.LineWidth = 2.0f;
            scatter.MarkerSize = 0f;
            scatter.LegendText = "Pickrate";
        }

        // graduations
        plot.Axes.Bottom.TickGenerator = new PatchTickGenerator(records);

        // echelle y
        var visibleValues = records
            .SelectMany(r => new[]
            {
                showBanRate ? (double?)r.BanRate : null,
                showWinRate ? (double?)r.WinRate : null,
                showPickRate ? (double?)r.PickRate : null
            })
            .Where(v => v.HasValue)
            .Select(v => v!.Value)
            .ToList();

        if (visibleValues.Count > 0)
        {
            double minY = Math.Max(0.0, visibleValues.Min() - 2.0);
            double maxY = visibleValues.Max() + 3.0;
            plot.Axes.SetLimits(0, records.Count - 1, minY, maxY);
        }
        else
        {
            plot.Axes.SetLimits(0, records.Count - 1, 0, 100);
        }
    }

    /// <summary>
    /// surligne le point clique sur la courbe correspondante
    /// </summary>
    public void HighlightPoint(Plot plot, BanRateRecord record, string metricName, double metricValue, string colorHex)
    {
        if (_selectedMarker != null)
        {
            plot.Remove(_selectedMarker);
        }
        if (_selectedAnnotation != null)
        {
            plot.Remove(_selectedAnnotation);
        }

        _selectedMarker = plot.Add.Marker(record.Index, metricValue);
        _selectedMarker.Color = Color.FromHex(colorHex);
        _selectedMarker.Size = 10f;
        _selectedMarker.Shape = MarkerShape.FilledCircle;

        _selectedAnnotation = plot.Add.Text(
            $"{record.Date:dd.MM.yyyy} (Patch {record.Patch})\n{metricName}: {metricValue:F2}%",
            record.Index,
            metricValue + 1.2);

        _selectedAnnotation.LabelFontColor = Color.FromHex("#0F172A");
        _selectedAnnotation.LabelFontSize = 12;
        _selectedAnnotation.LabelBold = true;
    }

    /// <summary>
    /// zoom avant
    /// </summary>
    public void ZoomIn(Plot plot) => plot.Axes.Zoom(1.3, 1.3);

    /// <summary>
    /// zoom arriere
    /// </summary>
    public void ZoomOut(Plot plot) => plot.Axes.Zoom(0.7, 0.7);

    /// <summary>
    /// reinitialise la vue
    /// </summary>
    public void ResetView(Plot plot, IReadOnlyList<BanRateRecord> records)
    {
        if (records == null || records.Count == 0)
        {
            plot.Axes.AutoScale();
            return;
        }

        double minBan = records.Min(r => r.BanRate);
        double maxBan = records.Max(r => r.BanRate);
        double yMin = Math.Max(0.0, minBan - 2.0);
        double yMax = maxBan + 3.5;

        plot.Axes.SetLimits(0, records.Count - 1, yMin, yMax);
    }
}
