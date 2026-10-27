using System;

namespace BanStatsApp.Models;

/// <summary>
/// represente une mesure de taux de ban lue depuis le jeu de donnees
/// </summary>
public class BanRateRecord
{
    /// <summary>
    /// index chronologique du point dans le graphique
    /// </summary>
    public int Index { get; set; }

    /// <summary>
    /// date de la mesure
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// version du patch de league of legends
    /// </summary>
    public string Patch { get; set; } = string.Empty;

    /// <summary>
    /// pourcentage de ban du champion
    /// </summary>
    public double BanRate { get; set; }

    /// <summary>
    /// retourne une chaine lisible decrivant la mesure
    /// </summary>
    public override string ToString()
    {
        return $"{Date:yyyy-MM-dd} (Patch {Patch}): {BanRate:F2}%";
    }
}
