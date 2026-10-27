using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using BanStatsApp.Models;
using BanStatsApp.Services;

namespace BanStatsApp.Controllers;

/// <summary>
/// controleur principal assurant la liaison des donnees les filtres et les actions
/// </summary>
public class MainWindowController : INotifyPropertyChanged
{
    private readonly CsvDataLoader _dataLoader = new();
    private readonly BanRateAnalyzer _analyzer = new();

    private List<BanRateRecord> _allRecords = new();
    private BanRateRecord? _selectedRecord;

    private bool _showBanRate = true;
    private string _selectedSeason = "All Seasons";
    private string _selectedPatch = "All Patches";
    private string _championName = "Gwen";
    private string _statusMessage = "Chargement des donnees";

    /// <summary>
    /// evenement declenche lors du changement de valeur d une propriete
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// notification envoyee pour redessiner le graphique
    /// </summary>
    public event Action? RequestPlotUpdate;

    /// <summary>
    /// notification envoyee pour surligner un point clique
    /// </summary>
    public event Action<BanRateRecord>? RequestPointHighlight;

    /// <summary>
    /// liste des enregistrements actuellement visibles apres filtrage
    /// </summary>
    public List<BanRateRecord> FilteredRecords { get; set; } = new();

    /// <summary>
    /// resume global des indicateurs statistiques
    /// </summary>
    public StatsSummary Summary { get; set; } = new();

    /// <summary>
    /// collection des saisons disponibles pour le filtre
    /// </summary>
    public ObservableCollection<string> AvailableSeasons { get; set; } = new();

    /// <summary>
    /// collection des patchs disponibles pour le filtre
    /// </summary>
    public ObservableCollection<string> AvailablePatches { get; set; } = new();

    /// <summary>
    /// point actuellement selectionne sur le graphique
    /// </summary>
    public BanRateRecord? SelectedRecord
    {
        get => _selectedRecord;
        set
        {
            _selectedRecord = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// indique si la courbe du taux de ban doit etre affichee
    /// </summary>
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

    /// <summary>
    /// nom de la saison actuellement selectionnee
    /// </summary>
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

    /// <summary>
    /// nom du patch actuellement selectionne
    /// </summary>
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

    /// <summary>
    /// nom du champion affiche dans l interface
    /// </summary>
    public string ChampionName
    {
        get => _championName;
        set
        {
            _championName = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// texte du message affiche dans la barre d etat
    /// </summary>
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
    /// charge les donnees initiales depuis le dossier data
    /// </summary>
    public void Initialize(string folder = "data")
    {
        // charge les enregistrements bruts depuis le premier csv trouve
        _allRecords = _dataLoader.LoadRecords(folder);

        if (_allRecords.Count == 0)
        {
            StatusMessage = "Aucun enregistrement trouve dans le fichier";
            return;
        }

        // remplit la liste des saisons disponibles
        AvailableSeasons.Clear();
        AvailableSeasons.Add("All Seasons");
        foreach (string season in _analyzer.GetAvailableSeasons(_allRecords))
        {
            AvailableSeasons.Add(season);
        }

        // remplit la liste des patchs disponibles
        AvailablePatches.Clear();
        AvailablePatches.Add("All Patches");
        foreach (string patch in _analyzer.GetUniquePatches(_allRecords))
        {
            AvailablePatches.Add(patch);
        }

        StatusMessage = $"{_allRecords.Count} enregistrements charges avec succes";
        ApplyFilters();

        // selectionne le point avec le taux le plus eleve par defaut
        BanRateRecord initialRecord = _allRecords.MaxBy(r => r.BanRate) ?? _allRecords.Last();
        SelectRecord(initialRecord);
    }

    /// <summary>
    /// applique les filtres de saison et patch puis met a jour les calculs
    /// </summary>
    public void ApplyFilters()
    {
        // filtre les donnees selon les criteres choisis
        IEnumerable<BanRateRecord> query = _allRecords;

        if (!string.IsNullOrEmpty(SelectedSeason) && SelectedSeason != "All Seasons")
        {
            query = _analyzer.FilterBySeason(query, SelectedSeason);
        }

        if (!string.IsNullOrEmpty(SelectedPatch) && SelectedPatch != "All Patches")
        {
            query = _analyzer.FilterByPatch(query, SelectedPatch);
        }

        FilteredRecords = query.OrderBy(r => r.Date).ToList();

        // reindexation des points pour le trace du graphique
        for (int i = 0; i < FilteredRecords.Count; i++)
        {
            FilteredRecords[i].Index = i;
        }

        // calcul des statistiques globales
        Summary = _analyzer.CalculateSummary(FilteredRecords);

        OnPropertyChanged(nameof(Summary));
        RequestPlotUpdate?.Invoke();
    }

    /// <summary>
    /// recherche le point le plus proche selon la coordonnee x cliquee
    /// </summary>
    public void SelectNearestPoint(double xCoord)
    {
        BanRateRecord? record = _analyzer.FindNearestRecord(FilteredRecords, xCoord);
        if (record != null)
        {
            SelectRecord(record);
        }
    }

    /// <summary>
    /// selectionne un enregistrement et demande l affichage du marqueur
    /// </summary>
    private void SelectRecord(BanRateRecord record)
    {
        SelectedRecord = record;
        RequestPointHighlight?.Invoke(record);
    }

    /// <summary>
    /// exporte les donnees filtrees sous forme de fichier csv
    /// </summary>
    public string ExportFilteredData(string? destinationPath = null)
    {
        string exportPath = destinationPath ?? Path.Combine(Directory.GetCurrentDirectory(), "ban_stats_export.csv");

        // generation des lignes du fichier texte
        List<string> lines = new List<string> { "Date,Patch,Banrate (%)" };
        IEnumerable<string> rows = FilteredRecords.Select(r => $"{r.Date:yyyy-MM-dd},{r.Patch},{r.BanRate:F2}");

        lines.AddRange(rows);
        File.WriteAllLines(exportPath, lines);

        StatusMessage = $"{lines.Count - 1} lignes exportees vers {Path.GetFileName(exportPath)}";
        return exportPath;
    }

    /// <summary>
    /// notifie la vue des changements de propriete
    /// </summary>
    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
