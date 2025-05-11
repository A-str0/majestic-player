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

        private readonly ObservableCollection<TorrentFileMetadata> _torrentFiles = [];
        public ObservableCollection<TorrentFileMetadata> TorrentFiles => _torrentFiles;

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

        private async Task ReloadTorrentFilesListAsync(TorrentResult torrentResult)
        {
            try
            {
                _torrentFiles.Clear();

                foreach (var file in await _searchService.GetTorrentMetadata(torrentResult))
                {
                    Debug.WriteLine($"Torrent file: {file.FilePath}");

                    _torrentFiles.Add(file);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Error in ReloadFilesAsync: {e.Message}");
                throw;
            }
        }

        public async Task ResultsList_ItemClick(object sender, ItemClickEventArgs e) 
        {
            Debug.WriteLine("ResultsListItem clicked");

            if (e.ClickedItem == null)
                throw new ArgumentNullException(nameof(e));

            await _searchService.PrepareTorrentManager(((TorrentResult)e.ClickedItem).MagnetLink);

            await ReloadTorrentFilesListAsync((TorrentResult)e.ClickedItem);
        }

        public async Task TorrentFiles_ItemClick(object sender, ItemClickEventArgs e)
        {
            Debug.WriteLine("TorrentFilesListItem clicked");

            TorrentFileMetadata metadata = (TorrentFileMetadata)e.ClickedItem;

            IHttpStream stream = await _searchService.StreamAsync(metadata.MagnetLink, metadata.FilePath);
            
            //Track track = _mediaHandlerService.GetTrackMetadataStream()

            await _audioService.PlayAsync(stream.FullUri.ToString());

        }
    }
}
