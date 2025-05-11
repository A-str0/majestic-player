using majestic_player.core.Models;

namespace majestic_player.core.Interfaces
{
    /// <summary>
    /// Service that controls media
    /// </summary>
    public interface IAudioService
    {
        public bool IsPlaying { get; }
        
        public event Action<Track>? TrackChanged;
        public event Action? EndReached;
        public event Action<bool>? PlayingStateChanged;
        
        public Task PlayAsync(Track track);
        public Task PlayAsync(Track track, string uri);

        public float GetCurrentPosition();

        public void PlayPause();
        public void SetVolume(float volume);
        public void SetPosition(float pos);

        public void Dispose();
    }
}