using majestic_player.core.Models;
using System.Security.Cryptography;
using majestic_player.infrastructure.Services;
using DynamicData;
using majestic_player.core.Interfaces;
using System.Diagnostics;
using System.Text.Json;
using System.IO;
using MonoTorrent.Client;
using MonoTorrent;
using MonoTorrent.Streaming;
using majestic_player.core.Enums;

public class MediaHandlerService(LibraryService libraryService) : IMediaHandlerService, IDisposable
{
    private readonly LibraryService _libraryService = libraryService;

    private static readonly string[] SupportedExtensions = { ".mp3", ".flac", ".wav", ".ogg" };

    private readonly SourceList<string> _folders = new SourceList<string>();
    public IObservable<IChangeSet<string>> Folders { get => _folders.Connect(); } // TODO: setting saving

    public async Task<IEnumerable<string>> GetLocalAudioFilesAsync(string folderPath)
    {
        return Directory.EnumerateFiles(folderPath, "*.*", SearchOption.AllDirectories)
            .Where(file => SupportedExtensions.Contains(Path.GetExtension(file).ToLower()));
    }

    /// <summary>
    /// Get local file metadata
    /// </summary>
    /// <param name="filePath">Path to file</param>
    /// <returns>Track object</returns>
    public Track GetTrackMetadataLocal(string filePath)
    {
        string trackHash = ComputeFileHash(filePath);

        try
        {
            using TagLib.File file = TagLib.File.Create(filePath);

            return new Track
            {
                Hash = trackHash,
                Title = file.Tag.Title ?? Path.GetFileNameWithoutExtension(filePath),
                Artist = file.Tag.FirstPerformer ?? "ADAPTIVEREADING",
                Album = file.Tag.Album,
                Duration = file.Properties.Duration,
                Year = (ushort)file.Tag.Year,
                Source = filePath,
                FileName = Path.GetFileNameWithoutExtension(filePath),
            };
        }
        catch (Exception e)
        {
            // TODO: Make unique Hash for tracks like this
            Debug.WriteLine(e);

            return new Track { Title = Path.GetFileName(filePath), Source = filePath, Hash = "Unknown" };
        }
    }

    // TODO: Move to another file
    public class StreamFileAbstraction : TagLib.File.IFileAbstraction
    {
        public StreamFileAbstraction(string name, Stream readStream, Stream writeStream)
        {
            // This TODO from TagLib source code:
            // https://github.com/timheuer/taglib-sharp-portable/blob/main/src/TagLib.Shared/TagLib/StreamFileAbstraction.cs
            // TODO: Fix deadlock when setting an actual writable Stream
            WriteStream = readStream;
            ReadStream = readStream;
            Name = name;
        }

        public string Name { get; private set; }

        public Stream ReadStream { get; private set; }

        public Stream WriteStream { get; private set; }

        public void CloseStream(Stream stream)
        {
            stream.Dispose();
        }
    }

    public async Task<Track> GetTrackMetadataTorrent(object m, object f, string magnetLink)
    {
        TorrentManager manager = (TorrentManager)m;
        ITorrentManagerFile file = (ITorrentManagerFile)f;

        try
        {

            using var stream = await manager.StreamProvider.CreateStreamAsync(file, false, CancellationToken.None);
            var buffer = new byte[128 * 1024];
            int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);

            using var memoryStream = new MemoryStream(buffer, 0, bytesRead);
            using var tagFile = TagLib.File.Create(new StreamFileAbstraction("temp_audio_path", memoryStream, memoryStream));
            return new Track
            {
                Title = tagFile.Tag.Title ?? file.Path,
                Artist = tagFile.Tag.FirstPerformer ?? "Unknown",
                Album = tagFile.Tag.Album ?? "Unknown",
                Duration = tagFile.Properties.Duration,
                Year = (ushort)(tagFile.Tag.Year > 0 ? tagFile.Tag.Year : 0),
                Source = magnetLink,
                FileName = file.Path,
                SourceType = SourceType.Torrent,
                Hash = ComputeStreamHash(stream) // Раскомментируйте и реализуйте, если нужен хэш
            };
        }
        catch (Exception e)
        {
            Debug.WriteLine($"Error extracting metadata: {e.Message}");
            return new Track
            {
                Title = Path.GetFileName(file.Path),
                FileName = file.Path,
                Source = magnetLink,
                Hash = "Unknown"
            };
        }
    }

    /// <summary>
    /// Scan folder for any audio files
    /// </summary>
    /// <param name="folderPath">Path to directory</param>
    /// <returns></returns>
    public async Task ScanFolderForAudioAsync(string folderPath)
    {
        Debug.WriteLine($"Scaning {folderPath} for audio files");

        List<Track> tracks = new List<Track>();
        foreach (var file in await GetLocalAudioFilesAsync(folderPath))
        {
            Track track = GetTrackMetadataLocal(file);

            tracks.Add(track);
        }

        await _libraryService.AddTracksAsync(tracks);
    }
    public void AddFolder(string folderPath)
    {
        Debug.WriteLine($"Adding folder: {folderPath}");
        _folders.Add(folderPath);
    }

    public string ComputeFileHash(string filePath)
    {
        using SHA256 sha256 = SHA256.Create();
        using FileStream stream = System.IO.File.OpenRead(filePath);
        byte[] hashBytes = sha256.ComputeHash(stream);
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
    }

    public string ComputeStreamHash(Stream stream)
    {
        using SHA256 sha256 = SHA256.Create();
        byte[] hashBytes = sha256.ComputeHash(stream);
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }
}