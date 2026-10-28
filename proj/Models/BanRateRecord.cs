// elias veya
// modele pour stocker les stats d'une ligne du csv

using System;

namespace BanStatsApp.Models;

public class BanRateRecord
{
    public int Index { get; set; }
    public DateTime Date { get; set; }
    public string Patch { get; set; } = string.Empty;

    // les differentes stats de jeu
    public double BanRate { get; set; }
    public double WinRate { get; set; }
    public double PickRate { get; set; }

    /// <summary>
    /// retourne le texte formate de la ligne
    /// </summary>
    public override string ToString()
    {
        return $"{Date:yyyy-MM-dd} (Patch {Patch}) | Ban: {BanRate:F2}% | Win: {WinRate:F2}% | Pick: {PickRate:F2}%";
    }
}
