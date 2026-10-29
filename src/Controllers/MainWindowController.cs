// elias veya
// controleur principal

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using BanStatsApp.Models;
using BanStatsApp.Services;
using BanStatsApp.Extensions;

namespace BanStatsApp.Controllers;

public class MainWindowController : INotifyPropertyChanged
{
    private readonly IDataLoader _dataLoader = new CsvDataLoader();
    private readonly BanRateAnalyzer _analyzer = new();

    private List<BanRateRecord> _allRecords = new();
    private BanRateRecord? _selectedRecord;

    // affichage des courbes
    private bool _showBanRate = true;
    private bool _showWinRate = false;
    private bool _showPickRate = false;

    // metrique selectionnee pour les stats du bas
    private string _selectedStatsMetric = "Banrate";

    // filtre par plage de dates
    private DateTime? _startDate;
    private DateTime? _endDate;

    private string _lastSelectedMetric = "Banrate";
    private double _lastSelectedValue = 0.0;
    private string _lastSelectedColor = "#E0247C";

    private string _selectedSeason = "All Seasons";
    private string _selectedPatch = "All Patches";
    private string _championName = "Gwen";
    private string _statusMessage = "pret";

    public event PropertyChangedEventHandler? PropertyChanged;
    public event Action? RequestPlotUpdate;
    public event Action<BanRateRecord, string, double, string>? RequestPointHighlight;

    public List<BanRateRecord> FilteredRecords { get; set; } = new();
    public StatsSummary Summary { get; set; } = new();

    public ObservableCollection<string> AvailableSeasons { get; set; } = new();
    public ObservableCollection<string> AvailablePatches { get; set; } = new();
    public ObservableCollection<string> AvailableChampions { get; set; } = new() { "Gwen", "Azir" };
    public ObservableCollection<string> AvailableStatsMetrics { get; set; } = new() { "Banrate", "Pickrate", "Winrate" };

    public BanRateRecord? SelectedRecord
    {
        get => _selectedRecord;
        set
        {
            _selectedRecord = value;
            OnPropertyChanged();
        }
    }

    public string SelectedStatsMetric
    {
        get => _selectedStatsMetric;
        set
        {
            if (_selectedStatsMetric != value)
            {
                _selectedStatsMetric = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ActiveStats));
                OnPropertyChanged(nameof(StatsTitle));
            }
        }
    }

    public MetricStats ActiveStats => SelectedStatsMetric switch
    {
        "Pickrate" => Summary.PickRate,
        "Winrate" => Summary.WinRate,
        _ => Summary.BanRate
    };

    public string StatsTitle => $"Global stats ({SelectedStatsMetric})";

    public DateTime? StartDate
    {
        get => _startDate;
        set
        {
            if (_startDate != value)
            {
                _startDate = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(StartDateOffset));
                ApplyFilters();
            }
        }
    }

    public DateTime? EndDate
    {
        get => _endDate;
        set
        {
            if (_endDate != value)
            {
                _endDate = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(EndDateOffset));
                ApplyFilters();
            }
        }
    }

    public DateTimeOffset? StartDateOffset
    {
        get => _startDate.HasValue ? new DateTimeOffset(_startDate.Value) : null;
        set => StartDate = value?.DateTime;
    }

    public DateTimeOffset? EndDateOffset
    {
        get => _endDate.HasValue ? new DateTimeOffset(_endDate.Value) : null;
        set => EndDate = value?.DateTime;
    }

    public string CurrentSelectedMetric => _lastSelectedMetric;
    public double CurrentSelectedValue => _lastSelectedValue;
    public string CurrentSelectedColor => _lastSelectedColor;

    public bool ShowBanRate
    {
        get => _showBanRate;
        set
        {
            if (_showBanRate != value)
            {
                _showBanRate = value;
                OnPropertyChanged();
                RequestPlotUpdate?.Invoke();
            }
        }
    }

    public bool ShowWinRate
    {
        get => _showWinRate;
        set
        {
            if (_showWinRate != value)
            {
                _showWinRate = value;
                OnPropertyChanged();
                RequestPlotUpdate?.Invoke();
            }
        }
    }

    public bool ShowPickRate
    {
        get => _showPickRate;
        set
        {
            if (_showPickRate != value)
            {
                _showPickRate = value;
                OnPropertyChanged();
                RequestPlotUpdate?.Invoke();
            }
        }
    }

    public string SelectedSeason
    {
        get => _selectedSeason;
        set
        {
            if (_selectedSeason != value)
            {
                _selectedSeason = value;
                OnPropertyChanged();
                ApplyFilters();
            }
        }
    }

    public string SelectedPatch
    {
        get => _selectedPatch;
        set
        {
            if (_selectedPatch != value)
            {
                _selectedPatch = value;
                OnPropertyChanged();
                ApplyFilters();
            }
        }
    }

    public string ChampionName
    {
        get => _championName;
        set
        {
            if (_championName != value)
            {
                _championName = value;
                OnPropertyChanged();
                LoadChampionData(_championName);
            }
        }
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set
        {
            _statusMessage = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// initialise les donnees au lancement
    /// </summary>
    public void Initialize(string folder = "data")
    {
        LoadChampionData(ChampionName, folder);
    }

    /// <summary>
    /// charge les donnees selon le champion selectionne
    /// </summary>
    public void LoadChampionData(string champion, string folder = "data")
    {
        string dirPath = Directory.Exists(folder) ? folder : Path.Combine(AppContext.BaseDirectory, folder);
        if (!Directory.Exists(dirPath))
        {
            StatusMessage = "dossier introuvable";
            return;
        }

        string prefix = champion.ToLowerInvariant();
        var files = Directory.GetFiles(dirPath, "*.csv");

        // cherche le fichier du champion
        string? targetFile = files.FirstOrDefault(f => Path.GetFileName(f).StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                             ?? files.FirstOrDefault();

        if (targetFile == null)
        {
            StatusMessage = $"aucun fichier pour {champion}";
            return;
        }

        LoadFromFile(targetFile);
    }

    /// <summary>
    /// lit les donnees du fichier csv specifie
    /// </summary>
    public void LoadFromFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            StatusMessage = "fichier introuvable";
            return;
        }

        _allRecords = _dataLoader.Load(filePath).ToList();

        if (_allRecords.Count == 0)
        {
            StatusMessage = "fichier vide";
            return;
        }

        // met a jour les filtres
        AvailableSeasons.Clear();
        AvailableSeasons.Add("All Seasons");
        _analyzer.GetAvailableSeasons(_allRecords)
            .ForEach(s => AvailableSeasons.Add(s));

        AvailablePatches.Clear();
        AvailablePatches.Add("All Patches");
        _analyzer.GetUniquePatches(_allRecords)
            .ForEach(p => AvailablePatches.Add(p));

        StatusMessage = $"{_allRecords.Count} points charges ({Path.GetFileName(filePath)})";
        ApplyFilters();

        var initialRecord = _allRecords.MaxBy(r => r.BanRate) ?? _allRecords.Last();
        SelectPointOnMetric(initialRecord, "Banrate", initialRecord.BanRate, "#E0247C");
    }

    /// <summary>
    /// reinitialise le filtre de dates
    /// </summary>
    public void ResetDateFilter()
    {
        _startDate = null;
        _endDate = null;
        OnPropertyChanged(nameof(StartDate));
        OnPropertyChanged(nameof(EndDate));
        OnPropertyChanged(nameof(StartDateOffset));
        OnPropertyChanged(nameof(EndDateOffset));
        ApplyFilters();
    }

    /// <summary>
    /// applique les filtres choisis par l'utilisateur
    /// </summary>
    public void ApplyFilters()
    {
        IEnumerable<BanRateRecord> query = _allRecords;

        // filtre par plage de dates
        query = query.FilterByDateRange(StartDate, EndDate);

        // filtre par saison et patch
        query = query.FilterBySeason(SelectedSeason);
        query = query.FilterByPatch(SelectedPatch);

        // reindexe les points
        FilteredRecords = query
            .OrderBy(r => r.Date)
            .Select((r, index) =>
            {
                r.Index = index;
                return r;
            })
            .ToList();

        Summary = _analyzer.CalculateSummary(FilteredRecords);

        OnPropertyChanged(nameof(Summary));
        OnPropertyChanged(nameof(ActiveStats));
        RequestPlotUpdate?.Invoke();
    }

    /// <summary>
    /// selectionne le point et la courbe les plus proches du clic
    /// </summary>
    public void SelectNearestPoint(double xCoord, double yCoord)
    {
        if (FilteredRecords.Count == 0)
        {
            return;
        }

        var record = _analyzer.FindNearestRecord(FilteredRecords, xCoord);
        if (record == null)
        {
            return;
        }

        var activeMetrics = new List<(string Name, double Value, string Color)>();
        if (ShowBanRate)
        {
            activeMetrics.Add(("Banrate", record.BanRate, "#E0247C"));
        }
        if (ShowWinRate)
        {
            activeMetrics.Add(("Winrate", record.WinRate, "#1E293B"));
        }
        if (ShowPickRate)
        {
            activeMetrics.Add(("Pickrate", record.PickRate, "#EF4444"));
        }

        if (activeMetrics.Count == 0)
        {
            activeMetrics.Add(("Banrate", record.BanRate, "#E0247C"));
        }

        var closest = activeMetrics.MinBy(m => Math.Abs(m.Value - yCoord));

        SelectPointOnMetric(record, closest.Name, closest.Value, closest.Color);
    }

    /// <summary>
    /// met a jour la selection du point et la notification graphique
    /// </summary>
    public void SelectPointOnMetric(BanRateRecord record, string metricName, double metricValue, string colorHex)
    {
        SelectedRecord = record;
        _lastSelectedMetric = metricName;
        _lastSelectedValue = metricValue;
        _lastSelectedColor = colorHex;

        // bascule les cartes de stats sur la metrique cliquee
        SelectedStatsMetric = metricName;

        RequestPointHighlight?.Invoke(record, metricName, metricValue, colorHex);

        StatusMessage = $"{record.Date:dd.MM.yyyy} (Patch {record.Patch}) • {metricName}: {metricValue:F2}% (Ban: {record.BanRate:F1}%, Win: {record.WinRate:F1}%, Pick: {record.PickRate:F1}%)";
    }

    /// <summary>
    /// notifie l'interface d'un changement de valeur
    /// </summary>
    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
