using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.DirectoryServices;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using majestic_player.core.Interfaces;
using majestic_player.core.Models;
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

        private ObservableCollection<Track> _allTracks = [];
        public ObservableCollection<Track> AllTracks => _allTracks;

        private ObservableCollection<string> _mediaFolders = [];
        public ObservableCollection<string> MediaFolders => _mediaFolders;

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
            _libraryService.LoadTracksAsync();
        }

        public void TracksList_ItemClick(object sender, ItemClickEventArgs e) => PlayTrack(e.ClickedItem as Track);

        private async Task ReloadTracksAsync(string folderPath)
        {
            try
            {
                _allTracks.Clear();

                foreach (var res in await _libraryService.GetAllTracksAsync())
                {
                    _allTracks.Add(res);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Error in SearchAsync: {e.Message}");
                throw;
            }
        }

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

                    await ReloadTracksAsync(path);
                }
            }
        }
    }
}
