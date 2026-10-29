// elias veya
// tests de chargement

using System;
using System.IO;
using Xunit;
using BanStatsApp.Services;

namespace BanStatsApp.Tests;

public class DataLoaderTests
{
    /// <summary>
    /// verifie la lecture correcte du fichier csv
    /// </summary>
    [Fact]
    public void CsvDataLoader_LoadsAndParsesRecords_Correctly()
    {
        // csv test
        string tempCsv = Path.GetTempFileName() + ".csv";
        try
        {
            File.WriteAllText(tempCsv, "Date,Patch,Banrate (%),pickrate (%),winrate (%)\n" +
                                       "2023-01-10,13.1,5.42,6.2,50.1\n" +
                                       "2023-01-11,13.1,6.00,7.0,51.0\n" +
                                       "2023-01-12,13.1,7.20,8.1,52.4\n");

            IDataLoader loader = new CsvDataLoader();

            var records = loader.Load(tempCsv);

            Assert.Equal(3, records.Count);
            Assert.Equal("13.1", records[0].Patch);
            Assert.Equal(5.42, records[0].BanRate);
            Assert.Equal(6.2, records[0].PickRate);
            Assert.Equal(50.1, records[0].WinRate);
        }
        finally
        {
            if (File.Exists(tempCsv))
            {
                File.Delete(tempCsv);
            }
        }
    }

    /// <summary>
    /// verifie le comportement si le fichier n'existe pas
    /// </summary>
    [Fact]
    public void CsvDataLoader_ReturnsEmpty_WhenFileNotFound()
    {
        IDataLoader loader = new CsvDataLoader();
        var records = loader.Load("fichier_inexistant.csv");
        Assert.Empty(records);
    }
}
