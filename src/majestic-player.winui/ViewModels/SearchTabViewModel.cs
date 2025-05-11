using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Runtime.InteropServices.Marshalling;
using System.Text;
using System.Threading.Tasks;
using majestic_player.core.Interfaces;
using majestic_player.core.Models;
using majestic_player.infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using MonoTorrent.Client;
using MonoTorrent.Streaming;
using ReactiveUI;

namespace majestic_player.winui.ViewModels
{
    public class SearchTabViewModel : ReactiveObject
    {
        #region Services
        private readonly LibraryService _libraryService;
        private readonly IMediaHandlerService _mediaHandlerService;
        private readonly PlaybackQueueService _playbackQueueService;
        private readonly IAudioService _audioService;
        private readonly TorrentSearchService _searchService;
        #endregion

        private readonly ObservableCollection<TorrentResult> _searchResults = [];
        public ObservableCollection<TorrentResult> SearchResults => _searchResults;

        private readonly ObservableCollection<Track> _torrentFiles = [];
        public ObservableCollection<Track> TorrentFiles => _torrentFiles;

        private string _searchQuery = "Linkin Park";
        public string SearchQuery
        {
            get => _searchQuery;
            set => this.RaiseAndSetIfChanged(ref _searchQuery, value);
        }

        public ReactiveCommand<Unit, Unit> SearchCommand { get; private set; }

        public SearchTabViewModel()
        {
            // Setup Commands
            SearchCommand = ReactiveCommand.CreateFromTask(SearchAsync);

            // Setup Services
            IServiceProvider serviceProvider = App.Services.CreateScope().ServiceProvider;

            _libraryService = serviceProvider.GetRequiredService<LibraryService>();
            _searchService = serviceProvider.GetRequiredService<TorrentSearchService>();
            _mediaHandlerService = serviceProvider.GetRequiredService<IMediaHandlerService>();
            _audioService = serviceProvider.GetRequiredService<IAudioService>();
            _playbackQueueService = serviceProvider.GetRequiredService<PlaybackQueueService>();
        }

        private async Task SearchAsync()
        {
            try
            {
                _searchResults.Clear();

                if (string.IsNullOrWhiteSpace(SearchQuery))
                {
                    Debug.WriteLine("Search query is empty");
                    return;
                }

                foreach (var res in await _searchService.SearchAsync(SearchQuery))
                {
                    _searchResults.Add(res);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Error in SearchAsync: {e.Message}");
                throw;
            }
        }

        TorrentManager torrentManager;
        public async Task ResultsList_ItemClick(object sender, ItemClickEventArgs e) 
        {
            Debug.WriteLine("ResultsListItem clicked");

            if (e.ClickedItem == null)
                throw new ArgumentNullException(nameof(e));

            TorrentResult torrentResult = (TorrentResult)e.ClickedItem;

            // TODO: Dispose torrent manager
            //if (torrentManager != null)
            //    torrentManager.
            torrentManager = await _searchService.PrepareTorrentManager(torrentResult.MagnetLink);

            _torrentFiles.Clear();
            foreach (var file in await _searchService.GetTorrentMetadata(torrentResult))
            {
                Debug.WriteLine($"Torrent file: {file.Title}, {file.FileName}");

                _torrentFiles.Add(file);
            }

            // TODO:
            //_playbackQueueService.CreateQueue(_torrentFiles[0], _torrentFiles);
        }

        public async Task TorrentFiles_ItemClick(object sender, ItemClickEventArgs e)
        {
            Debug.WriteLine("TorrentFilesListItem clicked");

            Track track = (Track)e.ClickedItem;

            Debug.WriteLine($"Playing: {track.Title} - {track.Artist} | {track.Album}");

            IHttpStream stream = await _searchService.StreamAsync(track.Source, track.FileName);

            await _audioService.PlayAsync(track, stream.FullUri.ToString());
        }
    }
}
