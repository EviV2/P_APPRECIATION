// elias veya
// tests des extensions

using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using BanStatsApp.Extensions;
using BanStatsApp.Models;

namespace BanStatsApp.Tests;

public class TimeSeriesExtensionsTests
{
    /// <summary>
    /// verifie le calcul de la moyenne glissante
    /// </summary>
    [Fact]
    public void MovingAverage_CalculatesRollingWindow_CorrectlyWithLinq()
    {
        // test moyenne
        var values = new List<double> { 10.0, 20.0, 30.0, 40.0 };

        var movingAvg = values.MovingAverage(2).ToList();

        Assert.Equal(4, movingAvg.Count);
        Assert.Equal(10.0, movingAvg[0]);
        Assert.Equal(15.0, movingAvg[1]);
        Assert.Equal(25.0, movingAvg[2]);
        Assert.Equal(35.0, movingAvg[3]);
    }

    /// <summary>
    /// verifie le filtre par plage de dates
    /// </summary>
    [Fact]
    public void FilterByDateRange_FiltersRecordsBetweenDates()
    {
        // test filtre date
        var records = new List<BanRateRecord>
        {
            new() { Date = new DateTime(2023, 8, 1), BanRate = 10.0 },
            new() { Date = new DateTime(2023, 8, 15), BanRate = 12.0 },
            new() { Date = new DateTime(2023, 12, 1), BanRate = 15.0 },
            new() { Date = new DateTime(2023, 12, 10), BanRate = 8.0 }
        };

        var filtered = records.FilterByDateRange(new DateTime(2023, 8, 2), new DateTime(2023, 12, 4)).ToList();

        Assert.Equal(2, filtered.Count);
        Assert.Equal(new DateTime(2023, 8, 15), filtered[0].Date);
        Assert.Equal(new DateTime(2023, 12, 1), filtered[1].Date);
    }

    /// <summary>
    /// verifie le filtre par saison
    /// </summary>
    [Fact]
    public void FilterBySeason_FiltersExpectedRecords()
    {
        // test filtre saison
        var records = new List<BanRateRecord>
        {
            new() { Patch = "12.1", Date = new DateTime(2022, 1, 10), BanRate = 4.0 },
            new() { Patch = "12.2", Date = new DateTime(2022, 1, 24), BanRate = 6.0 },
            new() { Patch = "13.1", Date = new DateTime(2023, 1, 10), BanRate = 10.0 }
        };

        var season12 = records.FilterBySeason("Season 12").ToList();

        Assert.Equal(2, season12.Count);
        Assert.All(season12, r => Assert.StartsWith("12.", r.Patch));
    }

    /// <summary>
    /// verifie le filtre par patch
    /// </summary>
    [Fact]
    public void FilterByPatch_FiltersExpectedRecords()
    {
        // test filtre patch
        var records = new List<BanRateRecord>
        {
            new() { Patch = "13.1", Date = new DateTime(2023, 1, 10), BanRate = 4.0 },
            new() { Patch = "13.2", Date = new DateTime(2023, 1, 24), BanRate = 6.0 }
        };

        var patch131 = records.FilterByPatch("13.1").ToList();

        Assert.Single(patch131);
        Assert.Equal("13.1", patch131[0].Patch);
    }
}
