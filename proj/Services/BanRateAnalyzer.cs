// elias veya
// calculs de stats

using System;
using System.Collections.Generic;
using System.Linq;
using BanStatsApp.Models;

namespace BanStatsApp.Services;

public class BanRateAnalyzer
{
    /// <summary>
    /// calcule les stats min max et moyenne sur les donnees
    /// </summary>
    public StatsSummary CalculateSummary(IEnumerable<BanRateRecord> records)
    {
        var recordList = records as IList<BanRateRecord> ?? records.ToList();
        if (recordList.Count == 0)
        {
            return new StatsSummary();
        }

        // banrate
        var minBan = recordList.MinBy(r => r.BanRate);
        var maxBan = recordList.MaxBy(r => r.BanRate);
        double avgBan = recordList.Average(r => r.BanRate);

        // pickrate
        var minPick = recordList.MinBy(r => r.PickRate);
        var maxPick = recordList.MaxBy(r => r.PickRate);
        double avgPick = recordList.Average(r => r.PickRate);

        // winrate
        var minWin = recordList.MinBy(r => r.WinRate);
        var maxWin = recordList.MaxBy(r => r.WinRate);
        double avgWin = recordList.Average(r => r.WinRate);

        var current = recordList.Last();

        return new StatsSummary
        {
            BanRate = new MetricStats
            {
                Current = current.BanRate,
                Min = minBan?.BanRate ?? 0.0,
                Max = maxBan?.BanRate ?? 0.0,
                Average = Math.Round(avgBan, 2),
                PeakPatch = maxBan?.Patch ?? string.Empty,
                LowestPatch = minBan?.Patch ?? string.Empty
            },
            PickRate = new MetricStats
            {
                Current = current.PickRate,
                Min = minPick?.PickRate ?? 0.0,
                Max = maxPick?.PickRate ?? 0.0,
                Average = Math.Round(avgPick, 2),
                PeakPatch = maxPick?.Patch ?? string.Empty,
                LowestPatch = minPick?.Patch ?? string.Empty
            },
            WinRate = new MetricStats
            {
                Current = current.WinRate,
                Min = minWin?.WinRate ?? 0.0,
                Max = maxWin?.WinRate ?? 0.0,
                Average = Math.Round(avgWin, 2),
                PeakPatch = maxWin?.Patch ?? string.Empty,
                LowestPatch = minWin?.Patch ?? string.Empty
            },
            TotalPoints = recordList.Count,
            MinDate = recordList.First().Date,
            MaxDate = recordList.Last().Date
        };
    }

    /// <summary>
    /// recupere la liste des patchs sans doublon
    /// </summary>
    public List<string> GetUniquePatches(IEnumerable<BanRateRecord> records)
    {
        return records
            .Select(r => r.Patch)
            .Distinct()
            .OrderBy(p => p)
            .ToList();
    }

    /// <summary>
    /// recupere la liste des saisons disponibles
    /// </summary>
    public List<string> GetAvailableSeasons(IEnumerable<BanRateRecord> records)
    {
        return records
            .Select(r => r.Patch.Split('.').FirstOrDefault() ?? string.Empty)
            .Where(s => !string.IsNullOrEmpty(s))
            .Distinct()
            .OrderBy(s => s)
            .Select(s => $"Season {s}")
            .ToList();
    }

    /// <summary>
    /// trouve le point le plus proche selon l'axe x
    /// </summary>
    public BanRateRecord? FindNearestRecord(IList<BanRateRecord> records, double xCoord)
    {
        if (records == null || records.Count == 0)
        {
            return null;
        }

        return records.MinBy(r => Math.Abs(r.Index - xCoord));
    }
}
