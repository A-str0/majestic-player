using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection.Metadata;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using DynamicData;
using majestic_player.core.Helpers;
using majestic_player.core.Interfaces;
using majestic_player.core.Models;
using MonoTorrent;
using MonoTorrent.Client;

namespace majestic_player.infrastructure.Services
{
    public class TorrentSearchService : ISearchService<TorrentResult>
    {
        // TODO: Remade
        private const string API = "apibay.org/q.php";
        private const int HTTP_LISTENING_PORT = 12345;
        private const string CAHCE_FOLDER_PATH = "C:\\Users\\magesty_\\AppData\\Roaming\\MajesticPlayer\\Cache\\";

        private readonly static HttpClient sharedClient = new HttpClient()
        {
            BaseAddress = new Uri(API),
        };

        private readonly SourceList<TorrentResult> _searchResult = new SourceList<TorrentResult>();
        public IObservable<IChangeSet<TorrentResult>> SearchResult { get => _searchResult.Connect(); }


        private readonly SourceList<Stream> _streams = new SourceList<Stream>();
        public IObservable<IChangeSet<Stream>> Streams { get => _streams.Connect(); }

        private IMediaHandlerService _mediaHandlerService;
        private string _routableAddress = $"http://127.0.0.1:{HTTP_LISTENING_PORT}/";

        public ClientEngine Engine { get; }
        

        public TorrentSearchService(IMediaHandlerService mediaHandlerService)
        {
            var settingBuilder = new EngineSettingsBuilder
            {
                AllowPortForwarding = true,

                AutoSaveLoadDhtCache = true,

                AutoSaveLoadFastResume = true,

                AutoSaveLoadMagnetLinkMetadata = true,

                ListenEndPoints = new Dictionary<string, IPEndPoint> {
                    { "ipv4", new IPEndPoint (IPAddress.Any, 55123) },
                    { "ipv6", new IPEndPoint (IPAddress.IPv6Any, 55123) }
                },

                DhtEndPoint = new IPEndPoint(IPAddress.Any, 55123),

                CacheDirectory = CAHCE_FOLDER_PATH,

                HttpStreamingPrefix = _routableAddress,
            };

            Engine = new ClientEngine(settingBuilder.ToSettings());

            _mediaHandlerService = mediaHandlerService;
        }

        public async Task SearchAsync(string query, string category = "101")
        {
            try
            {
                _searchResult.Clear();

                string formatedQuery = SearchQueryFormater.Format(query);

                using HttpResponseMessage response = await sharedClient.GetAsync($"?q={formatedQuery}&cat={category}");
                response.EnsureSuccessStatusCode();

                string jsonResponse = await response.Content.ReadAsStringAsync();
                // 0 - id; 1 - name; 2 - info_hash; 3 - leechers; 4 - seeders; 5 - num_files; 6 - size; 7 - username; 8 - added; 9 - status; 10 - category; 11 - imdb;
                JsonDocument doc = JsonDocument.Parse(jsonResponse);
                var torrents = doc.RootElement.EnumerateArray();

                foreach (var torrent in torrents)
                {
                    var infoHash = torrent.GetProperty("info_hash").GetString();
                    var result = new TorrentResult
                    {
                        Id = torrent.GetProperty("id").GetString(),
                        Title = torrent.GetProperty("name").GetString(),
                        InfoHash = infoHash,
                        MagnetLink = $"magnet:?xt=urn:btih:{infoHash}&dn={Uri.EscapeDataString(torrent.GetProperty("name").GetString())}",
                        Size = torrent.GetProperty("size").GetUInt64(),
                        Seeders = torrent.GetProperty("seeders").GetUInt32(),
                        Leechers = torrent.GetProperty("leechers").GetUInt32(),
                        NumFiles = torrent.GetProperty("num_files").GetUInt16(),
                        Username = torrent.GetProperty("username").GetString(),
                        Added = torrent.GetProperty("added").GetString(),
                        Status = torrent.GetProperty("status").GetString(),
                        Category = torrent.GetProperty("category").GetString(),
                        Imdb = torrent.GetProperty("imdb").GetString()
                    };

                    _searchResult.Add(result);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }

        public async Task SelectSearchResult(TorrentResult torrentResult)
        {
            CancellationTokenSource cancellation = new CancellationTokenSource();

            List<Stream> streams = await StreamAsync(MagnetLink.Parse(torrentResult.MagnetLink), cancellation.Token);

            await _mediaHandlerService.ScanStreamsForAudio(streams, "", torrentResult.MagnetLink);
        }

        private async Task<List<Stream>> StreamAsync(MagnetLink link, CancellationToken token)
        {
            // TODO: Cache and Downloads folder selection
            TorrentManager torrentManager = await Engine.AddStreamingAsync(link, "downloads");

            // Some debugging
            torrentManager.PeerConnected += (o, e) => { Debug.WriteLine("First peer connected"); };
            torrentManager.PeersFound += (o, e) => { Debug.WriteLine("Some peers found"); };
            torrentManager.PieceHashed += (o, e) => { Debug.WriteLine("Piece hashed"); };

            await torrentManager.StartAsync();
            await torrentManager.WaitForMetadataAsync(token);

            List<Stream> streams = new List<Stream>();
            foreach (var file in torrentManager.Files)
            {
                Stream stream = await torrentManager.StreamProvider.CreateStreamAsync(file, false);

                streams.Add(stream);
            }

            return streams;
        }
    }
}
