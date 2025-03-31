using LibVLCSharp.Shared;
using majestic_player.core.Interfaces;
using majestic_player.core.Models;

namespace majestic_player.infrastructure.Models
{
    public class AudioPlayer : IAudioService, IDisposable
    {
        private LibVLC _libVLC;
        private MediaPlayer _mediaPlayer;

        public bool IsPlaying { get => _mediaPlayer.IsPlaying; }

        public event Action<Track>? TrackChanged;
        public event Action? EndReached;

        public AudioPlayer()
        {
            Core.Initialize();
            
            _libVLC = new LibVLC();
            _mediaPlayer = new MediaPlayer(_libVLC);

            _mediaPlayer.EndReached += MediaPlayer_EndReached;
        }

        public async Task PlayAsync(Track track)
        {
            if (track == null) throw new ArgumentNullException(nameof(track));

            using var media = new Media(_libVLC, new Uri(track.Source));
            await Task.Run(() => _mediaPlayer.Play(media));
            
            TrackChanged?.Invoke(track);
        }

        public void PlayPause()
        {
            _mediaPlayer.SetPause(_mediaPlayer.IsPlaying);
        }

        public void SetVolume(float volume)
        {
            _mediaPlayer.Volume = (int)volume;
        }

        public void Dispose()
        {
            _mediaPlayer?.Dispose();
            _libVLC?.Dispose();
        }

        private void MediaPlayer_EndReached(object? sender, EventArgs e)
        {
            EndReached?.Invoke();
        }
    }
}
