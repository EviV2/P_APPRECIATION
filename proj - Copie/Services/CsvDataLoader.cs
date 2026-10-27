using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using BanStatsApp.Models;

namespace BanStatsApp.Services;

// charge les donnees depuis les fichiers csv
public class CsvDataLoader
{
    /// <summary>
    /// lit le premier fichier csv trouve dans le dossier data
    /// </summary>
    public List<BanRateRecord> LoadRecords(string folder = "data")
    {
        // recherche du dossier data
        string path = Directory.Exists(folder) ? folder : Path.Combine(AppContext.BaseDirectory, folder);
        if (!Directory.Exists(path))
        {
            return new List<BanRateRecord>();
        }

        // potentiellement rajoute possibilite pour plusieurs csv
        string? file = Directory.GetFiles(path, "*.csv").FirstOrDefault();
        if (file == null)
        {
            return new List<BanRateRecord>();
        }

        string[] lines = File.ReadAllLines(file);
        List<BanRateRecord> records = new List<BanRateRecord>();
        int index = 0;

        // boucle sur chaque ligne du fichier
        foreach (string line in lines.Skip(1))
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            string[] parts = line.Split(',');
            if (parts.Length < 3)
            {
                continue;
            }

            // lecture des colonnes date patch et pourcentage
            DateTime date = DateTime.Parse(parts[0].Trim());
            string patch = parts[1].Trim();
            double rate = double.Parse(parts[2].Trim(), CultureInfo.InvariantCulture);

            records.Add(new BanRateRecord
            {
                Index = index++,
                Date = date,
                Patch = patch,
                BanRate = rate
            });
        }

        // tri chronologique des enregistrements
        return records.OrderBy(r => r.Date).ToList();
    }
}
