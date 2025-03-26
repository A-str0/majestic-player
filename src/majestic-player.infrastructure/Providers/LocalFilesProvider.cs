using majestic_player.core.Enums;
using majestic_player.core.Interfaces;
using majestic_player.core.Models;

namespace majestic_player.infrastructure.Providers
{
    public class LocalFilesProvider : IMediaProvider
    {
        public SourceType SourceType { get; set; } = SourceType.Local;

        //TODO: разбораться что это вообще все такое и как использовать, ибо я забыл
        public async Task<Track> GetTrackMetadataAsync(string source)
        {
            FileInfo fileInfo = new FileInfo(source);

            return await Task.FromResult(new Track {
                Title = Path.GetFileNameWithoutExtension(source),
                Source = source,
                Hash = "Unknown"
            });
        }

        public async Task<IAudioSource> CreateAudioSourceAsync(string source)
        {
            // Implementation to create an audio source from local files
            // Placeholder for actual implementation
            return await Task.FromResult<IAudioSource>(null);
        }
    }
}
