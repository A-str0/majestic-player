﻿﻿﻿﻿﻿﻿﻿using Avalonia;
using System;
using Microsoft.Extensions.DependencyInjection;
using majestic_player.infrastructure.Models;
using majestic_player.infrastructure.Services;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace majestic_player.ui;

sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    public static ServiceProvider? Services { get; private set; }

    [STAThread]
    public static void Main(string[] args) 
    {
        ConfigureServices();

        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);

        Console.WriteLine("Ready");
    }

    public static void ConfigureServices()
    {
        var services = new ServiceCollection();
        
        services.AddDbContext<AppDBContext>();
        
        services.AddSingleton<AudioPlayer>();
        services.AddSingleton<LibraryService>();
        
        Services = services.BuildServiceProvider();
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}
