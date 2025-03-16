using System.IO;
using majestic_player.core.Models;
using TagLib; // Установите пакет TagLibSharp

public class FileScannerService
{
    private static readonly string[] SupportedExtensions = { ".mp3", ".flac", ".wav", ".ogg" };

    public IEnumerable<string> GetAudioFiles(string folderPath)
    {
        return Directory.EnumerateFiles(folderPath, "*.*", SearchOption.AllDirectories)
            .Where(file => SupportedExtensions.Contains(Path.GetExtension(file).ToLower()));
    }

    public Track GetTrackMetadata(string filePath)
    {
        try
        {
            using var file = TagLib.File.Create(filePath);
            return new Track
            {
                Title = file.Tag.Title ?? Path.GetFileNameWithoutExtension(filePath),
                Artist = file.Tag.FirstPerformer ?? "Неизвестный исполнитель",
                Album = file.Tag.Album ?? "Без альбома",
                Duration = file.Properties.Duration,
                // Year = (int)(file.Tag.Year > 0 ? file.Tag.Year : 0),
                Source = filePath
            };
        }
        catch (Exception ex)
        {
            // Логируйте ошибку
            return new Track { Title = Path.GetFileName(filePath), Source = filePath };
        }
    }
}