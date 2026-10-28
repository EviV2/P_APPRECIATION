// elias veya
// interface pour charger les fichiers

using System.Collections.Generic;
using BanStatsApp.Models;

namespace BanStatsApp.Services;

public interface IDataLoader
{
    /// <summary>
    /// charge les donnees d'un fichier csv
    /// </summary>
    IReadOnlyList<BanRateRecord> Load(string filePath);
}
