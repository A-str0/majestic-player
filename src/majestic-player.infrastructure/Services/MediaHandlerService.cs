using majestic_player.core.Models;
using System.Security.Cryptography;
using majestic_player.infrastructure.Services;
using DynamicData;
using majestic_player.core.Interfaces;

public class MediaHandlerService : IMediaHandlerService
{
    private readonly LibraryService _libraryService;

    private static readonly string[] SupportedExtensions = { ".mp3", ".flac", ".wav", ".ogg" };

    private readonly SourceList<string> _folders = new SourceList<string>();
    public IObservable<IChangeSet<string>> Folders { get => _folders.Connect(); }
 
    public MediaHandlerService(LibraryService libraryService)
    {
        _libraryService = libraryService;
    }

    public IEnumerable<string> GetAudioFiles(string folderPath)
    {
        return Directory.EnumerateFiles(folderPath, "*.*", SearchOption.AllDirectories)
            .Where(file => SupportedExtensions.Contains(Path.GetExtension(file).ToLower()));
    }

    public Track GetTrackMetadata(string filePath)
    {
        try
        {
            using TagLib.File file = TagLib.File.Create(filePath);

            return new Track
            {
                Hash = ComputeFileHash(filePath),
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
            // TODO: logging
            Console.WriteLine("EXCETPTION:", e);
            return new Track { Title = Path.GetFileName(filePath), Source = filePath, Hash = "Unknown" };
        }
    }

    public async Task ScanFolderForAudio(string folderPath)
    {
        foreach (var file in GetAudioFiles(folderPath))
        {
            var track = GetTrackMetadata(file);
            await _libraryService?.AddTrackAsync(track);
        }
    }

    public void AddFolder(string folderPath)
    {
        Console.WriteLine($"Adding folder: {folderPath}");
        _folders.Add(folderPath);
    }

    public string ComputeFileHash(string filePath)
    {
        using SHA256 sha256 = SHA256.Create();
        using FileStream stream = System.IO.File.OpenRead(filePath);
        byte[] hashBytes = sha256.ComputeHash(stream);
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
    }
}