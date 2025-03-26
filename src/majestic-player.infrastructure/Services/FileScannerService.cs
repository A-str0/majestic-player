using System.IO;
using majestic_player.core.Models;
using TagLib;
using System.Security.Cryptography;
using System.Threading.Tasks;

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
            using TagLib.File file = TagLib.File.Create(filePath);

            return new Track
            {
                Hash = ComputeFileHash(filePath),
                Title = file.Tag.Title ?? Path.GetFileNameWithoutExtension(filePath),
                Artist = file.Tag.FirstPerformer ?? "Unknown",
                Album = file.Tag.Album ?? "Unknown",
                Duration = file.Properties.Duration,
                // Year = (int)(file.Tag.Year > 0 ? file.Tag.Year : 0),
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

    private string ComputeFileHash(string filePath)
    {
        using SHA256 sha256 = SHA256.Create();
        using FileStream stream = System.IO.File.OpenRead(filePath);
        byte[] hashBytes = sha256.ComputeHash(stream);
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
    }
}