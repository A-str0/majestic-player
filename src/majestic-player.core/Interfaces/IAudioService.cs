using majestic_player.core.Models;

namespace majestic_player.core.Interfaces
{
    /// <summary>
    /// Service that controls media
    /// </summary>
    public interface IAudioService
    {
        public Track? CurrentTrack { get; protected set; }
        // TODO: Систему состояний плеера
        // public PlayerState State { get; protected set; }
        
        public event Action<Track>? TrackChanged;
        // event Action<PlayerState>? StateChanged;
        
        Task PlayAsync(Track track);
        void Pause();
        void Resume();
        void Stop();
        void SetVolume(float volume); // 0.0 - 1.0
    }
}