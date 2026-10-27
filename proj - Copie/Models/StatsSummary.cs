using System;

namespace BanStatsApp.Models;

/// <summary>
/// stocke les indicateurs globaux calcules sur le jeu de donnees complet
/// </summary>
public class StatsSummary
{
    /// <summary>
    /// taux de ban le plus recent
    /// </summary>
    public double CurrentRate { get; set; }

    /// <summary>
    /// taux de ban minimum observe sur toute la periode
    /// </summary>
    public double MinRate { get; set; }

    /// <summary>
    /// taux de ban maximum observe sur toute la periode
    /// </summary>
    public double MaxRate { get; set; }

    /// <summary>
    /// moyenne generale du taux de ban
    /// </summary>
    public double AverageRate { get; set; }

    /// <summary>
    /// nombre total de points de mesure disponibles
    /// </summary>
    public int TotalPoints { get; set; }

    /// <summary>
    /// premiere date enregistree dans le fichier
    /// </summary>
    public DateTime MinDate { get; set; }

    /// <summary>
    /// derniere date enregistree dans le fichier
    /// </summary>
    public DateTime MaxDate { get; set; }

    /// <summary>
    /// nom du patch ou le taux de ban a atteint son pic
    /// </summary>
    public string PeakPatch { get; set; } = string.Empty;

    /// <summary>
    /// nom du patch ou le taux de ban a ete le plus bas
    /// </summary>
    public string LowestPatch { get; set; } = string.Empty;
}
