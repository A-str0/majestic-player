using majestic_player.core.Models;
using System.Security.Cryptography;
using majestic_player.infrastructure.Services;
using DynamicData;
using majestic_player.core.Interfaces;
using System.Diagnostics;

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

    public async Task<IEnumerable<string>> GetAudioFilesAsync(string folderPath)
    {
        return Directory.EnumerateFiles(folderPath, "*.*", SearchOption.AllDirectories)
            .Where(file => SupportedExtensions.Contains(Path.GetExtension(file).ToLower()));
    }

    public async Task<Track> GetTrackMetadataAsync(string filePath)
    {
        try
        {
            string trackHash = await ComputeFileHashAsync(filePath);

            return await Task.Run(() =>
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
            });
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
            return await Task.Run(() =>
            {
                return new Track { Title = Path.GetFileName(filePath), Source = filePath, Hash = "Unknown" };
            });
        }
    }

    public async Task ScanFolderForAudioAsync(string folderPath)
    {
        Debug.WriteLine($"Scaning {folderPath} for audio files");

        IEnumerable<Track> tracks = new List<Track>();
        foreach (var file in await GetAudioFilesAsync(folderPath))
        {
            var track = await GetTrackMetadataAsync(file);
            tracks.Append(track);
        }
        await _libraryService.AddTracksAsync(tracks);
    }

    public void AddFolder(string folderPath)
    {
        Debug.WriteLine($"Adding folder: {folderPath}");
        _folders.Add(folderPath);
    }

    public async Task<string> ComputeFileHashAsync(string filePath)
    {
        return await Task.Run(() =>
        {
            using SHA256 sha256 = SHA256.Create();
            using FileStream stream = System.IO.File.OpenRead(filePath);
            byte[] hashBytes = sha256.ComputeHash(stream);
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        });
    }
}