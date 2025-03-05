using majestic_player.core.Enums;
using majestic_player.core.Models;

namespace majestic_player.core.Interfaces
{
    /// <summary>
    /// Interface for tracks indexation
    /// </summary>
    public interface IMediaProvider
    {
        public SourceType SourceType { get; protected set; } 
        
        public Task<Track> GetTrackMetadataAsync(string source);
        public Task<IAudioSource> CreateAudioSourceAsync(string source);
    }
}