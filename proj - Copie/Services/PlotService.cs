using System;
using System.Collections.Generic;
using System.Linq;
using ScottPlot;
using ScottPlot.Plottables;
using BanStatsApp.Models;

namespace BanStatsApp.Services;

// gestionnaire du rendu graphique scottplot du style et des marqueurs interactifs
public class PlotService
{
    private Scatter? _banRateScatter;
    private Marker? _selectedMarker;
    private Text? _selectedAnnotation;

    /// <summary>
    /// configure l'apparence visuelle du graphique les marges et les axes
    /// </summary>
    public void ConfigurePlotStyle(Plot plot)
    {
        // applique un theme clair et epure correspondant a la maquette
        plot.FigureBackground.Color = Color.FromHex("#FFFFFF");
        plot.DataBackground.Color = Color.FromHex("#FFFFFF");

        // masque la legende interne car deja presente dans le panneau lateral
        plot.Legend.IsVisible = false;

        // masque le compteur de performance et de fps
        plot.Benchmark.IsVisible = false;

        // regle les lignes de quadrillage pour etre tres discretes
        plot.Grid.MajorLineColor = Color.FromHex("#F1F5F9");
        plot.Grid.MajorLineWidth = 1;

        // ajuste les etiquettes et tailles minimales des axes pour eviter tout chevauchement
        plot.Axes.Left.Label.Text = "Banrate (%)";
        plot.Axes.Left.Label.Bold = true;
        plot.Axes.Left.Label.FontSize = 13;
        plot.Axes.Left.Label.ForeColor = Color.FromHex("#334155");
        plot.Axes.Left.MinimumSize = 55;

        plot.Axes.Bottom.Label.Text = "League Patch / Date";
        plot.Axes.Bottom.Label.Bold = true;
        plot.Axes.Bottom.Label.FontSize = 13;
        plot.Axes.Bottom.Label.ForeColor = Color.FromHex("#334155");
        plot.Axes.Bottom.MinimumSize = 65;
        plot.Axes.Bottom.TickLabelStyle.FontSize = 11;
    }

    /// <summary>
    /// met a jour la courbe et les graduations a partir des donnees filtrees
    /// </summary>
    public void UpdateSeries(
        Plot plot,
        IReadOnlyList<BanRateRecord> records,
        bool showBanRate)
    {
        plot.Clear();
        _banRateScatter = null;
        _selectedMarker = null;
        _selectedAnnotation = null;

        if (records == null || records.Count == 0)
        {
            return;
        }

        // extraction des coordonnees x et y
        double[] xs = records.Select(r => (double)r.Index).ToArray();
        double[] ys = records.Select(r => r.BanRate).ToArray();

        // ajoute la courbe principale du taux de ban en rose vif
        if (showBanRate)
        {
            _banRateScatter = plot.Add.Scatter(xs, ys);
            _banRateScatter.Color = Color.FromHex("#E0247C");
            _banRateScatter.LineWidth = 2.4f;
            _banRateScatter.MarkerSize = 0f;
        }

        // associe le generateur de graduations sans chevauchement
        plot.Axes.Bottom.TickGenerator = new PatchTickGenerator(records);

        // recadre les axes pour cadrer parfaitement les pourcentages de 0 a la valeur max
        double minBan = records.Min(r => r.BanRate);
        double maxBan = records.Max(r => r.BanRate);
        double yMin = Math.Max(0.0, minBan - 1.5);
        double yMax = maxBan + 3.5;

        plot.Axes.SetLimits(0, records.Count - 1, yMin, yMax);
    }

    /// <summary>
    /// place un marqueur visuel et une etiquette au point selectionne
    /// </summary>
    public void HighlightPoint(Plot plot, BanRateRecord record)
    {
        // retire le marqueur precedent s'il existe
        if (_selectedMarker != null)
        {
            plot.Remove(_selectedMarker);
        }
        if (_selectedAnnotation != null)
        {
            plot.Remove(_selectedAnnotation);
        }

        // dessine le point cible
        _selectedMarker = plot.Add.Marker(record.Index, record.BanRate);
        _selectedMarker.Color = Color.FromHex("#E0247C");
        _selectedMarker.Size = 10f;
        _selectedMarker.Shape = MarkerShape.FilledCircle;

        // ajoute l'annotation textuelle au dessus du point
        _selectedAnnotation = plot.Add.Text($"{record.BanRate:F1}%", record.Index, record.BanRate + 1.2);
        _selectedAnnotation.LabelFontColor = Color.FromHex("#0F172A");
        _selectedAnnotation.LabelFontSize = 13;
        _selectedAnnotation.LabelBold = true;
    }

    /// <summary>
    /// zoome a l'interieur du graphique
    /// </summary>
    public void ZoomIn(Plot plot)
    {
        plot.Axes.Zoom(1.3, 1.3);
    }

    /// <summary>
    /// dezoome pour elargir la vue
    /// </summary>
    public void ZoomOut(Plot plot)
    {
        plot.Axes.Zoom(0.7, 0.7);
    }

    /// <summary>
    /// reinitialise la vue pour voir l'ensemble des points
    /// </summary>
    public void ResetView(Plot plot, IReadOnlyList<BanRateRecord> records)
    {
        if (records == null || records.Count == 0)
        {
            plot.Axes.AutoScale();
            return;
        }

        // recadre sur l'ensemble complet des donnees
        double minBan = records.Min(r => r.BanRate);
        double maxBan = records.Max(r => r.BanRate);
        double yMin = Math.Max(0.0, minBan - 1.5);
        double yMax = maxBan + 3.5;

        plot.Axes.SetLimits(0, records.Count - 1, yMin, yMax);
    }
}
