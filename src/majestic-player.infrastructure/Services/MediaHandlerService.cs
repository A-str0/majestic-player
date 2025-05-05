using majestic_player.core.Models;
using System.Security.Cryptography;
using majestic_player.infrastructure.Services;
using DynamicData;
using majestic_player.core.Interfaces;
using System.Diagnostics;
using System.Text.Json;
using System.IO;
using MonoTorrent.Client;
using TagLib.Mpeg;
using MonoTorrent;

public class MediaHandlerService : IMediaHandlerService
{
    private readonly LibraryService _libraryService;

    private static readonly string[] SupportedExtensions = { ".mp3", ".flac", ".wav", ".ogg" };

    private readonly SourceList<string> _folders = new SourceList<string>();
    public IObservable<IChangeSet<string>> Folders { get => _folders.Connect(); } // TODO: setting saving
 
    public MediaHandlerService(LibraryService libraryService)
    {
        _libraryService = libraryService;
    }

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
                Source = filePath
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

    /// <summary>
    /// Get file metadata in Stream
    /// </summary>
    /// <param name="stream">Stream where file locates</param>
    /// <param name="filePath">Path to file in stream</param>
    /// <param name="magnetLink"></param>
    /// <returns>Track object</returns>
    public async Task<Track> GetTrackMetadataStream(Stream stream, string filePath, string magnetLink)
    {
        try
        {
            using (stream)
            {
                using var memoryStream = new MemoryStream();
                await stream.CopyToAsync(memoryStream);
                memoryStream.Position = 0;

                string trackHash = ComputeStreamHash(memoryStream);

                using var file = TagLib.File.Create(new StreamFileAbstraction(Path.GetFileName(filePath), memoryStream, memoryStream));
                return new Track
                {
                    Hash = trackHash,
                    Title = file.Tag.Title ?? Path.GetFileNameWithoutExtension(filePath),
                    Artist = file.Tag.FirstPerformer ?? "ADAPTIVEREADING",
                    Album = file.Tag.Album,
                    Duration = file.Properties.Duration,
                    Year = (ushort)file.Tag.Year,
                    Source = magnetLink
                };
            }
        }
        catch (Exception e)
        {
            Debug.WriteLine($"Error extracting metadata: {e.Message}");
            return new Track
            {
                Title = Path.GetFileName(filePath),
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

    public async Task ScanStreamsForAudio(List<Stream> streams, string filePath, string magnetLink)
    {
        var tracks = new List<Track>();
        foreach (var stream in streams)
        {
            Track track = await GetTrackMetadataStream(stream, filePath, magnetLink);

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
}