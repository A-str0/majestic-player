using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using DynamicData;
using majestic_player.core.Interfaces;
using majestic_player.core.Models;
using majestic_player.infrastructure.Models;
using majestic_player.infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using ReactiveUI;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace majestic_player.winui.ViewModels
{
    public partial class LibraryTabViewModel : ReactiveObject
    {
        #region Services
        private readonly IAudioService _audioService;
        private readonly LibraryService _libraryService;
        private readonly IMediaHandlerService _mediaHandlerService;
        private readonly PlaybackQueueService _playbackQueueService;
        #endregion

        private ReadOnlyObservableCollection<Track> _allTracks;
        public ReadOnlyObservableCollection<Track> AllTracks => _allTracks;

        private ReadOnlyObservableCollection<string> _mediaFolders;
        public ReadOnlyObservableCollection<string> MediaFolders => _mediaFolders;

        public ReactiveCommand<Unit, Unit> AddFolderCommand { get; private set; }

        public LibraryTabViewModel()
        {
            // Setup Commands
            AddFolderCommand = ReactiveCommand.CreateFromTask(AddFolderDialogAsync);

            // Setup Services
            IServiceProvider serviceProvider = App.Services.CreateScope().ServiceProvider;

            _audioService = serviceProvider.GetRequiredService<IAudioService>();

            _libraryService = serviceProvider.GetRequiredService<LibraryService>();
            _mediaHandlerService = serviceProvider.GetRequiredService<IMediaHandlerService>();
            _playbackQueueService = serviceProvider.GetRequiredService<PlaybackQueueService>();

            // Setup Observers
            _mediaHandlerService?.Folders
                .ObserveOn(RxApp.MainThreadScheduler)
                .Bind(out _mediaFolders)
                .Do(_ => UpdateTracksAsync()?.GetAwaiter().GetResult())
                .DisposeMany()
            .Subscribe();

            _libraryService.LoadTracksAsync();

            _libraryService?.Tracks
                .ObserveOn(RxApp.MainThreadScheduler)
                .Bind(out _allTracks)
                .DisposeMany()
                .Subscribe();
        }

        public void TracksList_ItemClick(object sender, ItemClickEventArgs e) => PlayTrack(e.ClickedItem as Track);

        public async Task PlayTrack(Track track)
        {
            if (track == null)
            {
                Debug.WriteLine("Track is null");
                return;
            }

            Debug.WriteLine($"Track {track.Title} is playing");

            if (_playbackQueueService.Queue.Count() == 0)
            {
                Debug.WriteLine($"Queue was created");

                _playbackQueueService.CreateQueue(track, AllTracks);
            }

            await _audioService.PlayAsync(track);
        }

        public async Task AddFolderDialogAsync()
        {
            Debug.WriteLine($"Add folder dialog appeared");

            var folderPicker = new FolderPicker
            {
                ViewMode = PickerViewMode.List
            };
            folderPicker.FileTypeFilter.Add("*");

            IntPtr hwnd = WindowNative.GetWindowHandle(App.CurrentWindow);
            InitializeWithWindow.Initialize(folderPicker, hwnd);

            var folder = await folderPicker.PickSingleFolderAsync();

            if (folder != null)
            {
                string path = folder.Path;
                if (!string.IsNullOrEmpty(path))
                {
                    _mediaHandlerService.AddFolder(path);

                    await _mediaHandlerService.ScanFolderForAudioAsync(path);
                }
            }

            //await UpdateTracksAsync();
        }

        private async Task? UpdateTracksAsync()
        {
            Console.WriteLine("Updating tracks due to folder change...");
            await _libraryService?.LoadTracksAsync();
            Console.WriteLine("Tracks updated");
        }
    }
}
