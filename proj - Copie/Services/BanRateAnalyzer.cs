using System;
using System.Collections.Generic;
using System.Linq;
using BanStatsApp.Models;

namespace BanStatsApp.Services;

// effectue les analyses de donnees et calculs statistiques
public class BanRateAnalyzer
{
    /// <summary>
    /// calcule le resume statistique global
    /// </summary>
    public StatsSummary CalculateSummary(IEnumerable<BanRateRecord> records)
    {
        List<BanRateRecord> recordList = records.ToList();
        if (!recordList.Any())
        {
            return new StatsSummary();
        }

        // extraction des extreme et de la moyenne
        BanRateRecord? minRecord = recordList.MinBy(r => r.BanRate);
        BanRateRecord? maxRecord = recordList.MaxBy(r => r.BanRate);
        double avgRate = recordList.Average(r => r.BanRate);
        BanRateRecord current = recordList.Last();

        return new StatsSummary
        {
            CurrentRate = current.BanRate,
            MinRate = minRecord?.BanRate ?? 0,
            MaxRate = maxRecord?.BanRate ?? 0,
            AverageRate = avgRate,
            TotalPoints = recordList.Count,
            MinDate = recordList.First().Date,
            MaxDate = recordList.Last().Date,
            PeakPatch = maxRecord?.Patch ?? string.Empty,
            LowestPatch = minRecord?.Patch ?? string.Empty
        };
    }

    /// <summary>
    /// recupere l'ensemble des noms de patchs uniques
    /// </summary>
    public List<string> GetUniquePatches(IEnumerable<BanRateRecord> records)
    {
        // extraction distincte des patchs
        return records
            .Select(r => r.Patch)
            .Distinct()
            .ToList();
    }

    /// <summary>
    /// identifie les differentes saisons a partir des prefixes de patch
    /// </summary>
    public List<string> GetAvailableSeasons(IEnumerable<BanRateRecord> records)
    {
        // decoupe le numero de patch pour trouver la saison
        return records
            .Select(r => r.Patch.Split('.').FirstOrDefault() ?? "")
            .Where(s => !string.IsNullOrEmpty(s))
            .Distinct()
            .OrderBy(s => s)
            .Select(s => $"Season {s}")
            .ToList();
    }

    /// <summary>
    /// filtre les enregistrements pour un patch precis
    /// </summary>
    public List<BanRateRecord> FilterByPatch(IEnumerable<BanRateRecord> records, string patch)
    {
        return records
            .Where(r => string.Equals(r.Patch, patch, StringComparison.OrdinalIgnoreCase))
            .OrderBy(r => r.Date)
            .ToList();
    }

    /// <summary>
    /// filtre les donnees par saison de jeu
    /// </summary>
    public List<BanRateRecord> FilterBySeason(IEnumerable<BanRateRecord> records, string seasonName)
    {
        if (string.IsNullOrWhiteSpace(seasonName) || seasonName == "All Seasons")
        {
            return records.OrderBy(r => r.Date).ToList();
        }

        string prefix = seasonName.Replace("Season", "").Trim();
        return records
            .Where(r => r.Patch.StartsWith(prefix + ".", StringComparison.OrdinalIgnoreCase))
            .OrderBy(r => r.Date)
            .ToList();
    }

    /// <summary>
    /// recherche le point le plus proche d'une coordonnee x cliquee
    /// </summary>
    public BanRateRecord? FindNearestRecord(IList<BanRateRecord> records, double xCoord)
    {
        if (records == null || records.Count == 0)
        {
            return null;
        }

        // tri par distance minimale absolue
        return records
            .OrderBy(r => Math.Abs(r.Index - xCoord))
            .FirstOrDefault();
    }
}
