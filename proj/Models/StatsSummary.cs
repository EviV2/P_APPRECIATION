// elias veya
// stocke les totaux et calculs pour l'affichage

using System;

namespace BanStatsApp.Models;

public class MetricStats
{
    public double Current { get; set; }
    public double Min { get; set; }
    public double Max { get; set; }
    public double Average { get; set; }
    public string PeakPatch { get; set; } = string.Empty;
    public string LowestPatch { get; set; } = string.Empty;
}

public class StatsSummary
{
    // proprietes banrate de base
    public double CurrentRate => BanRate.Current;
    public double MinRate => BanRate.Min;
    public double MaxRate => BanRate.Max;
    public double AverageRate => BanRate.Average;
    public string PeakPatch => BanRate.PeakPatch;
    public string LowestPatch => BanRate.LowestPatch;

    // stats detaillees par metrique
    public MetricStats BanRate { get; set; } = new();
    public MetricStats PickRate { get; set; } = new();
    public MetricStats WinRate { get; set; } = new();

    public int TotalPoints { get; set; }
    public DateTime MinDate { get; set; }
    public DateTime MaxDate { get; set; }
}
