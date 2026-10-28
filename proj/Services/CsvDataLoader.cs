// elias veya
// chargeur csv

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using BanStatsApp.Models;

namespace BanStatsApp.Services;

public class CsvDataLoader : IDataLoader
{
    // todo: gerer les autres separateurs comme le point virgule
    // todo: charger en asynchrone si le fichier devient trop lourd

    /// <summary>
    /// charge les donnees d'un fichier csv en memoire
    /// </summary>
    public IReadOnlyList<BanRateRecord> Load(string filePath)
    {
        if (!File.Exists(filePath))
        {
            return Array.Empty<BanRateRecord>();
        }

        // lit les lignes du fichier
        return File.ReadLines(filePath)
            .Skip(1)
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Select(line => line.Split(','))
            .Where(parts => parts.Length >= 3)
            .Select((parts, index) => ParseRow(parts, index))
            .Where(record => record != null)
            .Select(record => record!)
            .OrderBy(r => r.Date)
            .ToList();
    }

    /// <summary>
    /// parse une ligne de texte vers un enregistrement
    /// </summary>
    private static BanRateRecord? ParseRow(string[] parts, int index)
    {
        // parse la ligne standard
        if (DateTime.TryParse(parts[0].Trim(), CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
        {
            string patch = parts[1].Trim();

            string banRateRaw = parts[2].Trim().Replace(',', '.');
            if (!double.TryParse(banRateRaw, NumberStyles.Any, CultureInfo.InvariantCulture, out var banRate))
            {
                return null;
            }

            double pickRate = 8.0;
            if (parts.Length > 3)
            {
                string pickRaw = parts[3].Trim().Replace(',', '.');
                if (double.TryParse(pickRaw, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsedPick))
                {
                    pickRate = parsedPick;
                }
            }

            double winRate = 50.0;
            if (parts.Length > 4)
            {
                string winRaw = parts[4].Trim().Replace(',', '.');
                if (double.TryParse(winRaw, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsedWin))
                {
                    winRate = parsedWin;
                }
            }

            return new BanRateRecord
            {
                Index = index,
                Date = date,
                Patch = patch,
                BanRate = banRate,
                PickRate = pickRate,
                WinRate = winRate
            };
        }

        // secours pour le format modifie
        if (parts.Length >= 5 &&
            double.TryParse(parts[0].Trim() + "." + parts[1].Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var parsedBan) &&
            double.TryParse(parts[3].Trim().Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out var parsedPick2) &&
            double.TryParse(parts[4].Trim().Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out var parsedWin2))
        {
            var fallbackDate = new DateTime(2023, 1, 10).AddDays(index);
            string fallbackPatch = $"13.{Math.Min(24, 1 + index / 15)}";

            return new BanRateRecord
            {
                Index = index,
                Date = fallbackDate,
                Patch = fallbackPatch,
                BanRate = parsedBan,
                PickRate = parsedPick2,
                WinRate = parsedWin2
            };
        }

        return null;
    }
}
