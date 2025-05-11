using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection.Metadata;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using majestic_player.core.Helpers;
using majestic_player.core.Interfaces;
using majestic_player.core.Models;
using MonoTorrent;
using MonoTorrent.Client;
using MonoTorrent.Streaming;

namespace majestic_player.infrastructure.Services
{
    public class TorrentSearchService : ISearchService<TorrentResult>
    {
        // TODO: Remade
        private const string CAHCE_FOLDER_PATH = "C:\\Users\\magesty_\\AppData\\Roaming\\MajesticPlayer\\Cache\\"; // TODO: Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)
        private const int HTTP_LISTENING_PORT = 12345;

        private readonly string _routableAddress = $"http://127.0.0.1:{HTTP_LISTENING_PORT}/";
        private IHttpStream httpStream = null;

        private const string API = "http://apibay.org/";
        private readonly static HttpClient sharedClient = new HttpClient()
        {
            BaseAddress = new Uri(API),
        };

        private readonly Dictionary<string, TorrentManager> _torrentManagers = new Dictionary<string, TorrentManager>();
        private readonly IMediaHandlerService _mediaHandlerService;

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

                UsePartialFiles = true,
            };

            Engine = new ClientEngine(settingBuilder.ToSettings());

            _mediaHandlerService = mediaHandlerService;
        }

        public async Task<List<TorrentResult>> SearchAsync(string query, string category = "101")
        {
            try
            {
                string formatedQuery = SearchQueryFormater.Format(query);

                Debug.WriteLine($"Searching for: {formatedQuery}");

                using HttpResponseMessage response = await sharedClient.GetAsync($"q.php?q={formatedQuery}&cat={category}");
                response.EnsureSuccessStatusCode();

                Debug.WriteLine($"API Response: {response.StatusCode}");

                string jsonResponse = await response.Content.ReadAsStringAsync();
                // 0 - id; 1 - name; 2 - info_hash; 3 - leechers; 4 - seeders; 5 - num_files; 6 - size; 7 - username; 8 - added; 9 - status; 10 - category; 11 - imdb;
                JsonDocument doc = JsonDocument.Parse(jsonResponse);
                var torrents = doc.RootElement.EnumerateArray();

                List<TorrentResult> results = new List<TorrentResult>();
                foreach (var torrent in torrents)
                { 
                    Debug.WriteLine($"Creating TorrentResult: {torrent}");

                    var infoHash = torrent.GetProperty("info_hash").GetString();
                    var result = new TorrentResult
                    {
                        Id = torrent.GetProperty("id").GetString(),
                        Title = torrent.GetProperty("name").GetString(),
                        InfoHash = infoHash,
                        MagnetLink = $"magnet:?xt=urn:btih:{infoHash}&dn={Uri.EscapeDataString(torrent.GetProperty("name").GetString())}",
                        Size = ParseUInt64(torrent.GetProperty("size")),
                        Seeders = ParseUInt32(torrent.GetProperty("seeders")),
                        Leechers = ParseUInt32(torrent.GetProperty("leechers")),
                        NumFiles = ParseUInt16(torrent.GetProperty("num_files")),
                        Username = torrent.GetProperty("username").GetString(),
                        Added = torrent.GetProperty("added").GetString(),
                        Status = torrent.GetProperty("status").GetString(),
                        Category = torrent.GetProperty("category").GetString(),
                        Imdb = torrent.GetProperty("imdb").GetString()
                    };

                    Debug.WriteLine($"TorrentResult: {result.Id}");

                    results.Add(result);
                }

                return results;
            }
            catch (Exception e)
            {
                Debug.WriteLine("ATTENTION!!!");
                Debug.WriteLine(e);

                return new List<TorrentResult> { };
            }
        }

        private static ulong ParseUInt64(JsonElement element) => element.ValueKind == JsonValueKind.Number ? element.GetUInt64() : ulong.Parse(element.GetString());
        private static uint ParseUInt32(JsonElement element) => element.ValueKind == JsonValueKind.Number ? element.GetUInt32() : uint.Parse(element.GetString());
        private static ushort ParseUInt16(JsonElement element) => element.ValueKind == JsonValueKind.Number ? element.GetUInt16() : ushort.Parse(element.GetString());

        public async Task<TorrentManager> PrepareTorrentManager(string magnetLink)
        {
            if (string.IsNullOrWhiteSpace(magnetLink))
                throw new ArgumentNullException(nameof(magnetLink));

            Debug.WriteLine($"Preparing TorrentManager for {magnetLink}");

            if (_torrentManagers.TryGetValue(magnetLink, out TorrentManager? value))
            {
                Debug.WriteLine($"TorrentManager for {magnetLink} already exists");
                return value;
            }
            else
            {
                TorrentManager torrentManager = await Engine.AddStreamingAsync(MagnetLink.Parse(magnetLink), Path.GetTempPath());

                _torrentManagers[magnetLink] = torrentManager;  

                await torrentManager.StartAsync();

                Debug.WriteLine($"TorrentManager for {magnetLink} was created");

                return torrentManager;
            }
        }

        public async Task<List<Track>> GetTorrentMetadata(TorrentResult torrentResult)
        {
            var magnetLink = torrentResult.MagnetLink;
            TorrentManager torrentManager = _torrentManagers[magnetLink];

            Debug.WriteLine($"Downloading metadata...");

            // TOOD: Remade this
            // Some debugging
            torrentManager.PeerConnected += (o, e) => { Debug.WriteLine("First peer connected"); };
            torrentManager.PeersFound += (o, e) => { Debug.WriteLine("Peer found"); };
            torrentManager.PieceHashed += (o, e) => { Debug.WriteLine("Piece hashed"); };
            torrentManager.TorrentStateChanged += (o, e) => { Debug.WriteLine($"Torrent state changed: {e.NewState}"); };

            if (torrentManager.State == TorrentState.Stopped)
                await torrentManager.StartAsync();

            await torrentManager.WaitForMetadataAsync(CancellationToken.None); // TODO: CancellationToken

            List<Track> tracks = new List<Track>();
            foreach (var file in torrentManager.Files)
            {
                if (torrentManager.State == TorrentState.Stopped)
                    await torrentManager.StartAsync();

                Track track = new Track() 
                { 
                    Title = file.Path,
                    Source = magnetLink,
                    FileName = file.Path,
                    SourceType = core.Enums.SourceType.Torrent,
                };

                //Track track = await _mediaHandlerService.GetTrackMetadataTorrent(torrentManager, file, magnetLink);
                tracks.Add(track);

                await torrentManager.SetFilePriorityAsync(file, Priority.DoNotDownload);
            }

            Debug.WriteLine($"Metadata downloaded. Files count: {tracks.Count}");

            return tracks;
        }

        public async Task<IHttpStream> StreamAsync(string magnetLink, string fileName)
        {
            TorrentManager torrentManager = _torrentManagers[magnetLink];

            await torrentManager.WaitForMetadataAsync(CancellationToken.None); // TODO: CancellationToken

            ITorrentManagerFile fileToDownload = torrentManager.Files.FirstOrDefault(f => f.Path == fileName);
            if (fileToDownload == null)
            {
                Debug.WriteLine($"File {fileName} was not found!");
                throw new NullReferenceException();
            }

            // Set priorities
            await torrentManager.SetFilePriorityAsync(fileToDownload, Priority.Normal);
            foreach (var file in torrentManager.Files.Where(f => f.Path != fileName))
            {
                await torrentManager.SetFilePriorityAsync(file, Priority.DoNotDownload);
            }

            if (torrentManager.State == TorrentState.Stopped)
                await torrentManager.StartAsync();

            httpStream?.Dispose();
            httpStream = await torrentManager.StreamProvider.CreateHttpStreamAsync(fileToDownload, CancellationToken.None); // TODO: CancellationToken

            return httpStream;
        }
    }

}
