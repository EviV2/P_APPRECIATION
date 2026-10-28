// elias veya
// tests des calculs

using System;
using System.Collections.Generic;
using Xunit;
using BanStatsApp.Models;
using BanStatsApp.Services;

namespace BanStatsApp.Tests;

public class BanRateAnalyzerTests
{
    private readonly BanRateAnalyzer _analyzer = new();

    /// <summary>
    /// verifie les calculs statistiques min max et moyenne
    /// </summary>
    [Fact]
    public void CalculateSummary_ComputesAccurateAggregations()
    {
        // test stats
        var records = new List<BanRateRecord>
        {
            new() { Index = 0, Date = new DateTime(2023, 1, 1), Patch = "13.1", BanRate = 5.0 },
            new() { Index = 1, Date = new DateTime(2023, 1, 2), Patch = "13.2", BanRate = 25.0 },
            new() { Index = 2, Date = new DateTime(2023, 1, 3), Patch = "13.3", BanRate = 15.0 }
        };

        var summary = _analyzer.CalculateSummary(records);

        Assert.Equal(5.0, summary.MinRate);
        Assert.Equal(25.0, summary.MaxRate);
        Assert.Equal(15.0, summary.AverageRate);
        Assert.Equal(15.0, summary.CurrentRate);
        Assert.Equal("13.2", summary.PeakPatch);
        Assert.Equal("13.1", summary.LowestPatch);
        Assert.Equal(3, summary.TotalPoints);
    }

    /// <summary>
    /// verifie la recherche du point le plus proche
    /// </summary>
    [Fact]
    public void FindNearestRecord_ReturnsRecordWithClosestIndex()
    {
        // test point proche
        var records = new List<BanRateRecord>
        {
            new() { Index = 0, BanRate = 5.0 },
            new() { Index = 10, BanRate = 12.0 },
            new() { Index = 20, BanRate = 8.0 }
        };

        var near9 = _analyzer.FindNearestRecord(records, 9.2);
        Assert.NotNull(near9);
        Assert.Equal(10, near9.Index);

        var near17 = _analyzer.FindNearestRecord(records, 16.8);
        Assert.NotNull(near17);
        Assert.Equal(20, near17.Index);
    }
}
