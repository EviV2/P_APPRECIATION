// elias veya
// methodes d'extension utiles pour les calculs

using System;
using System.Collections.Generic;
using System.Linq;
using BanStatsApp.Models;

namespace BanStatsApp.Extensions;

public static class TimeSeriesExtensions
{
    /// <summary>
    /// calcule la moyenne glissante sur n elements
    /// </summary>
    public static IEnumerable<double> MovingAverage(this IEnumerable<double> source, int windowSize)
    {
        if (windowSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(windowSize), "taille invalide");
        }

        var values = source as IList<double> ?? source.ToList();
        if (values.Count == 0)
        {
            return Enumerable.Empty<double>();
        }

        return Enumerable.Range(0, values.Count)
            .Select(i =>
            {
                int start = Math.Max(0, i - windowSize + 1);
                int count = i - start + 1;
                return Math.Round(values.Skip(start).Take(count).Average(), 2);
            });
    }

    /// <summary>
    /// filtre les enregistrements selon une plage de dates
    /// </summary>
    public static IEnumerable<BanRateRecord> FilterByDateRange(this IEnumerable<BanRateRecord> records, DateTime? startDate, DateTime? endDate)
    {
        var query = records;
        if (startDate.HasValue)
        {
            query = query.Where(r => r.Date >= startDate.Value.Date);
        }
        if (endDate.HasValue)
        {
            query = query.Where(r => r.Date <= endDate.Value.Date);
        }
        return query.OrderBy(r => r.Date);
    }

    /// <summary>
    /// filtre les enregistrements selon une saison
    /// </summary>
    public static IEnumerable<BanRateRecord> FilterBySeason(this IEnumerable<BanRateRecord> records, string seasonName)
    {
        if (string.IsNullOrWhiteSpace(seasonName) || seasonName == "All Seasons")
        {
            return records.OrderBy(r => r.Date);
        }

        string prefix = seasonName.Replace("Season", "").Trim();
        return records
            .Where(r => r.Patch.StartsWith(prefix + ".", StringComparison.OrdinalIgnoreCase))
            .OrderBy(r => r.Date);
    }

    /// <summary>
    /// filtre les enregistrements selon un patch precis
    /// </summary>
    public static IEnumerable<BanRateRecord> FilterByPatch(this IEnumerable<BanRateRecord> records, string patch)
    {
        if (string.IsNullOrWhiteSpace(patch) || patch == "All Patches")
        {
            return records.OrderBy(r => r.Date);
        }

        return records
            .Where(r => string.Equals(r.Patch, patch, StringComparison.OrdinalIgnoreCase))
            .OrderBy(r => r.Date);
    }
}
