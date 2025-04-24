using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using System;
using System.Reactive;
using System.Windows.Input;
using ReactiveUI;
using majestic_player.core.Models;
using System.Threading.Tasks;
using majestic_player.core.Interfaces;
using majestic_player.infrastructure.Services;
using Windows.Storage;
using Microsoft.Extensions.DependencyInjection;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace majestic_player.winui
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        #region Services
        private readonly LibraryService? _libraryService;
        private readonly IMediaHandlerService? _mediaHandlerService;
        private readonly PlaybackQueueService? _playbackQueueService;
        private readonly IAudioService? _audioService;
        #endregion

        #region Commands
        public ReactiveCommand<Unit, Unit> AddFolderCommand { get; private set; }
        public ReactiveCommand<Track?, Unit> PlayTrackCommand { get; private set; }
        public ICommand PlayPauseCommand { get; private set; }
        public ICommand NextCommand { get; private set; }
        public ICommand PreviousCommand { get; private set; }
        #endregion

        public MainWindow()
        {
            Console.WriteLine("MainWindow initialization started");

            // Setup Commands
            //AddFolderCommand = ReactiveCommand.CreateFromTask(AddFolderDialog);
            //PlayTrackCommand = ReactiveCommand.CreateFromTask<Track?>(PlayTrack);

            //PlayPauseCommand = ReactiveCommand.Create(PlayPause);
            NextCommand = ReactiveCommand.Create(PlayNextTrackInQueue);
            PreviousCommand = ReactiveCommand.Create(PlayPreviousTrackInQueue);


            // Setup Services
            IServiceProvider serviceProvider = Program.Services.CreateScope().ServiceProvider;

            _audioService = serviceProvider.GetRequiredService<IAudioService>();

            _libraryService = serviceProvider.GetRequiredService<LibraryService>();
            _mediaHandlerService = serviceProvider.GetRequiredService<IMediaHandlerService>();
            _playbackQueueService = serviceProvider.GetRequiredService<PlaybackQueueService>();


            this.InitializeComponent();
        }

        public async Task? PlayNextTrackInQueue()
        {
            Track? nextTrack = _playbackQueueService?.ToNextTrackInQueue();
            await _audioService?.PlayAsync(nextTrack);
        }

        public async Task? PlayPreviousTrackInQueue()
        {
            Track? prevTrack = _playbackQueueService?.ToPreviousTrackInQueue();
            await _audioService?.PlayAsync(prevTrack);
        }
    }
}
