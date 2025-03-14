using Avalonia;
using System;
using Microsoft.Extensions.DependencyInjection;
using majestic_player.infrastructure.Models;
using majestic_player.infrastructure.Services;

namespace majestic_player.ui;

sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    public static ServiceCollection? Services { get; private set; }

    [STAThread]
    public static void Main(string[] args) 
    {
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);

        AppDBContext context = new AppDBContext();

        Services = new ServiceCollection();

        Services.AddDbContext<AppDBContext>();
        Services.AddSingleton<AudioPlayer>();
        Services.AddSingleton<LibraryService>();
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}
