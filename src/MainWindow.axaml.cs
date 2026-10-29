// elias veya
// fenetre principale

using System;
using System.IO;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using ScottPlot;
using BanStatsApp.Models;
using BanStatsApp.Services;
using BanStatsApp.Controllers;

namespace BanStatsApp;

public partial class MainWindow : Window
{
    private readonly MainWindowController _controller;
    private readonly PlotService _plotService;

    /// <summary>
    /// constructeur de la fenetre principale
    /// </summary>
    public MainWindow()
    {
        InitializeComponent();

        _plotService = new PlotService();
        _controller = new MainWindowController();
        DataContext = _controller;

        // style du graphe
        _plotService.ConfigurePlotStyle(MainPlot.Plot);

        MainPlot.UserInputProcessor.RemoveAll<ScottPlot.Interactivity.UserActionResponses.DoubleClickBenchmark>();

        // events
        _controller.RequestPlotUpdate += OnRequestPlotUpdate;
        _controller.RequestPointHighlight += OnRequestPointHighlight;

        // clic sur le graphe
        MainPlot.PointerPressed += OnPlotPointerPressed;

        // boutons
        ZoomInButton.Click += (_, _) =>
        {
            _plotService.ZoomIn(MainPlot.Plot);
            MainPlot.Refresh();
        };

        ZoomOutButton.Click += (_, _) =>
        {
            _plotService.ZoomOut(MainPlot.Plot);
            MainPlot.Refresh();
        };

        ResetZoomButton.Click += (_, _) =>
        {
            _plotService.ResetView(MainPlot.Plot, _controller.FilteredRecords);
            MainPlot.Refresh();
        };

        ResetDatesButton.Click += (_, _) =>
        {
            _controller.ResetDateFilter();
        };

        ImportButton.Click += OnImportClick;

        Loaded += (_, _) => _controller.Initialize("data");
    }

    /// <summary>
    /// gestion du clic utilisateur sur le graphique
    /// </summary>
    private void OnPlotPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        PointerPoint point = e.GetCurrentPoint(MainPlot);
        Pixel pixel = new Pixel((float)point.Position.X, (float)point.Position.Y);
        Coordinates coords = MainPlot.Plot.GetCoordinates(pixel);

        // cherche la courbe cliquee
        _controller.SelectNearestPoint(coords.X, coords.Y);
    }

    /// <summary>
    /// redessine le graphique quand les options changent
    /// </summary>
    private void OnRequestPlotUpdate()
    {
        _plotService.UpdateSeries(
            MainPlot.Plot,
            _controller.FilteredRecords,
            _controller.ShowBanRate,
            _controller.ShowWinRate,
            _controller.ShowPickRate);

        if (_controller.SelectedRecord != null)
        {
            _plotService.HighlightPoint(
                MainPlot.Plot,
                _controller.SelectedRecord,
                _controller.CurrentSelectedMetric,
                _controller.CurrentSelectedValue,
                _controller.CurrentSelectedColor);
        }

        MainPlot.Refresh();
    }

    /// <summary>
    /// met en valeur un point selectionne
    /// </summary>
    private void OnRequestPointHighlight(BanRateRecord record, string metricName, double metricValue, string colorHex)
    {
        _plotService.HighlightPoint(MainPlot.Plot, record, metricName, metricValue, colorHex);
        MainPlot.Refresh();
    }

    /// <summary>
    /// ouvre un fichier csv choisi par l'utilisateur
    /// </summary>
    private async void OnImportClick(object? sender, RoutedEventArgs e)
    {
        TopLevel? topLevel = TopLevel.GetTopLevel(this);
        if (topLevel == null) return;

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "importer un fichier csv",
            AllowMultiple = false,
            FileTypeFilter = new[]
            {
                new FilePickerFileType("fichiers csv (*.csv)")
                {
                    Patterns = new[] { "*.csv" }
                }
            }
        });

        if (files.Count > 0)
        {
            string path = files[0].Path.LocalPath;
            _controller.LoadFromFile(path);
        }
    }
}
