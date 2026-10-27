// Elias Veya
// Cid3A
// Application pour afficher des informations dans un graphes fetch depuis un fichier csv avec scottplot et avalonia
// Titre: Ban stats app

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

// code behind gerant les interactions souris les evenements et les liaisons vue modele
public partial class MainWindow : Window
{
    private readonly MainWindowController _controller;
    private readonly PlotService _plotService;

    /// <summary>
    /// initialise les composants graphiques configure les abonnements et le controleur
    /// </summary>
    public MainWindow()
    {
        InitializeComponent();

        _plotService = new PlotService();
        _controller = new MainWindowController();
        DataContext = _controller;

        // applique le theme visuel et le style du graphique
        _plotService.ConfigurePlotStyle(MainPlot.Plot);

        // desactive le double clic qui affiche le benchmark fps
        MainPlot.UserInputProcessor.RemoveAll<ScottPlot.Interactivity.UserActionResponses.DoubleClickBenchmark>();

        // abonnements aux notifications du controleur pour rafraichir le trace
        _controller.RequestPlotUpdate += OnRequestPlotUpdate;
        _controller.RequestPointHighlight += OnRequestPointHighlight;

        // liaison du clic utilisateur sur le graphique
        MainPlot.PointerPressed += OnPlotPointerPressed;

        // liaison des boutons d'actions
        ZoomInButton.Click += OnZoomInClick;
        ZoomOutButton.Click += OnZoomOutClick;
        ResetZoomButton.Click += OnResetZoomClick;
        ExportButton.Click += OnExportClick;

        // chargement des donnees des que la fenetre est prete
        Loaded += OnWindowLoaded;
    }

    /// <summary>
    /// charge le premier csv a l ouverture de la fenetre
    /// </summary>
    private void OnWindowLoaded(object? sender, RoutedEventArgs e)
    {
        _controller.Initialize("data");
    }

    /// <summary>
    /// gere le clic sur le graphique pour selectionner et mettre en valeur le point le plus proche
    /// </summary>
    private void OnPlotPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        PointerPoint point = e.GetCurrentPoint(MainPlot);
        Pixel pixel = new Pixel((float)point.Position.X, (float)point.Position.Y);
        Coordinates coords = MainPlot.Plot.GetCoordinates(pixel);

        // demande au controleur de selectionner le point le plus proche par son indice
        _controller.SelectNearestPoint(coords.X);
    }

    /// <summary>
    /// redessine les courbes et graduations quand les filtres changent
    /// </summary>
    private void OnRequestPlotUpdate()
    {
        _plotService.UpdateSeries(
            MainPlot.Plot,
            _controller.FilteredRecords,
            _controller.ShowBanRate);

        // reaffiche le point selectionne si actif
        if (_controller.SelectedRecord != null)
        {
            _plotService.HighlightPoint(MainPlot.Plot, _controller.SelectedRecord);
        }

        MainPlot.Refresh();
    }

    /// <summary>
    /// ajoute le marqueur visuel pour le point clique
    /// </summary>
    private void OnRequestPointHighlight(BanRateRecord record)
    {
        _plotService.HighlightPoint(MainPlot.Plot, record);
        MainPlot.Refresh();
    }

    /// <summary>
    /// effectue un zoom avant dans le graphique
    /// </summary>
    private void OnZoomInClick(object? sender, RoutedEventArgs e)
    {
        _plotService.ZoomIn(MainPlot.Plot);
        MainPlot.Refresh();
    }

    /// <summary>
    /// effectue un zoom arriere dans le graphique
    /// </summary>
    private void OnZoomOutClick(object? sender, RoutedEventArgs e)
    {
        _plotService.ZoomOut(MainPlot.Plot);
        MainPlot.Refresh();
    }

    /// <summary>
    /// reinitialise le zoom pour cadrer tous les points
    /// </summary>
    private void OnResetZoomClick(object? sender, RoutedEventArgs e)
    {
        _plotService.ResetView(MainPlot.Plot, _controller.FilteredRecords);
        MainPlot.Refresh();
    }

    /// <summary>
    /// exporte les donnees filtrees dans un fichier csv sur le disque
    /// </summary>
    private async void OnExportClick(object? sender, RoutedEventArgs e)
    {
        TopLevel? topLevel = TopLevel.GetTopLevel(this);
        if (topLevel != null)
        {
            IStorageFile? file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "Exporter les donnees",
                SuggestedFileName = "ban_stats_export.csv",
                DefaultExtension = "csv"
            });

            if (file != null)
            {
                _controller.ExportFilteredData(file.Path.LocalPath);
                return;
            }
        }

        // enregistrement par defaut dans le dossier courant
        string fallbackPath = Path.Combine(Directory.GetCurrentDirectory(), "ban_stats_export.csv");
        _controller.ExportFilteredData(fallbackPath);
    }
}
