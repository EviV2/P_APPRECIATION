using System;
using System.Collections.Generic;
using ScottPlot;
using ScottPlot.TickGenerators;
using SkiaSharp;
using BanStatsApp.Models;

namespace BanStatsApp.Services;

// affiche les patchs et dates sur l axe du bas
public class PatchTickGenerator : ITickGenerator
{
    private readonly IReadOnlyList<BanRateRecord> _records;

    public Tick[] Ticks { get; set; } = Array.Empty<Tick>();
    public int MaxTickCount { get; set; } = 6;

    /// <summary>
    /// constructeur recevant la liste des donnees
    /// </summary>
    public PatchTickGenerator(IReadOnlyList<BanRateRecord> records)
    {
        _records = records;
    }

    /// <summary>
    /// calcule les graduations visibles
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

        int step = Math.Max(1, (end - start) / 6);
        List<Tick> tickList = new List<Tick>();

        // boucle sur les enregistrements a afficher
        for (int i = start; i <= end; i += step)
        {
            BanRateRecord r = _records[i];
            string label = $"{r.Patch}\n{r.Date:yyyy-MM-dd}";
            tickList.Add(new Tick(r.Index, label, true));
        }

        Ticks = tickList.ToArray();
    }
}
