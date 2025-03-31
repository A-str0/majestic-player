using majestic_player.core.Models;

namespace majestic_player.core.Interfaces
{
    /// <summary>
    /// Service that controls media
    /// </summary>
    public interface IAudioService
    {
        // TODO: Систему состояний плеера
        // public PlayerState State { get; protected set; }
        
        public event Action<Track>? TrackChanged;
        // event Action<PlayerState>? StateChanged;
        
        Task PlayAsync(Track track);
        void PlayPause();
        void SetVolume(float volume); // 0.0 - 1.0
    }
}