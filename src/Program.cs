// elias veya
// point d'entree du programme

using Avalonia;
using System;

namespace BanStatsApp;

class Program
{
    /// <summary>
    /// point d'entree principal de l'application
    /// </summary>
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);

    /// <summary>
    /// configuration d'avalonia
    /// </summary>
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}
