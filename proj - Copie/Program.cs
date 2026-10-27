// Elias Veya
// Cid3A
// Application pour afficher des informations dans un graphes fetch depuis un fichier csv avec scottplot et avalonia
// Titre: Ban stats app

using Avalonia;
using System;

namespace BanStatsApp;

class Program
{
    /// <summary>
    /// point d'entree principal de l'application desktop
    /// </summary>
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);

    /// <summary>
    /// configure et initialise l'application avalonia
    /// </summary>
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}
