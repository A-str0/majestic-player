using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using majestic_player.core.Interfaces;
using majestic_player.infrastructure.Models;
using majestic_player.infrastructure.Services;
using majestic_player.winui.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace majestic_player.winui
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        public static Window? CurrentWindow => (Current as App)?.m_window;

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
        }

        public static ServiceProvider Services { get; private set; }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            ConfigureServices();

            m_window = new MainWindow
            {
                DataContext = new MainWindowViewModel(),
            };
            m_window.Activate();
        }

        /// <summary>
        /// Method for configuring DI services singletons
        /// </summary>
        public static void ConfigureServices()
        {
            var services = new ServiceCollection();

            services.AddDbContextFactory<AppDBContext>();

            services.AddSingleton<IAudioService, AudioService>();
            services.AddSingleton<LibraryService>();
            services.AddSingleton<IMediaHandlerService, MediaHandlerService>();
            services.AddSingleton<PlaybackQueueService>();
            services.AddSingleton<TorrentSearchService>();

            Services = services.BuildServiceProvider();
        }

        private Window? m_window;
    }
}
