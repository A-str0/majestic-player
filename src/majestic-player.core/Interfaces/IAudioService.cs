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
        
        public Task PlayAsync(Track track);
        public void PlayPause();
        public void SetVolume(float volume);


        public void Dispose();
    }
}