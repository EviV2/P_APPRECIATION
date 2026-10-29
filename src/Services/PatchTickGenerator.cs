// elias veya
// generation des graduations de l'axe du bas pour scottplot

using System;
using System.Collections.Generic;
using System.Linq;
using ScottPlot;
using ScottPlot.TickGenerators;
using SkiaSharp;
using BanStatsApp.Models;

namespace BanStatsApp.Services;

public class PatchTickGenerator : ITickGenerator
{
    private readonly IReadOnlyList<BanRateRecord> _records;

    public Tick[] Ticks { get; set; } = Array.Empty<Tick>();
    public int MaxTickCount { get; set; } = 6;

    /// <summary>
    /// constructeur recevant la liste des enregistrements
    /// </summary>
    public PatchTickGenerator(IReadOnlyList<BanRateRecord> records)
    {
        _records = records;
    }

    /// <summary>
    /// recalcule les graduations visibles selon le zoom
    /// </summary>
    public void Regenerate(CoordinateRange range, Edge edge, PixelLength size, SKPaint paint, LabelStyle labelStyle)
    {
        if (_records.Count == 0)
        {
            Ticks = Array.Empty<Tick>();
            return;
        }

        int start = Math.Max(0, (int)range.Min);
        int end = Math.Min(_records.Count - 1, (int)range.Max);

        if (end <= start)
        {
            Ticks = Array.Empty<Tick>();
            return;
        }

        int step = Math.Max(1, (end - start) / MaxTickCount);
        int count = ((end - start) / step) + 1;

        // calcule les labels patch + date
        Ticks = Enumerable.Range(0, count)
            .Select(i => start + (i * step))
            .Where(idx => idx <= end && idx < _records.Count)
            .Select(idx => _records[idx])
            .Select(r => new Tick(r.Index, $"{r.Patch}\n{r.Date:yyyy-MM-dd}", true))
            .ToArray();
    }
}
