using majestic_player.core.Enums;
using majestic_player.core.Interfaces;
using majestic_player.core.Models;

namespace majestic_player.infrastructure.Providers
{
    public class LocalFilesProvider : IMediaProvider
    {
        public SourceType SourceType { get; set; } = SourceType.Local;

        public async Task<Track> GetTrackMetadataAsync(string source)
        {
            FileInfo fileInfo = new FileInfo(source);

            return await Task.FromResult(new Track {
                Title = Path.GetFileNameWithoutExtension(source),
                Source = source
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
