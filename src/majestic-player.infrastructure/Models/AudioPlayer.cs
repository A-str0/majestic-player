using LibVLCSharp.Shared;
using majestic_player.core.Interfaces;
using majestic_player.core.Models;
using System;
using System.Threading.Tasks;

namespace majestic_player.infrastructure.Models
{
    public class AudioPlayer : IAudioService, IDisposable
    {
        private LibVLC _libVLC;
        private MediaPlayer _mediaPlayer;

        public Track? CurrentTrack { get; set; }

        public event Action<Track>? TrackChanged;

        public AudioPlayer()
        {
            Core.Initialize(); // Initialize LibVLC
            _libVLC = new LibVLC();
            _mediaPlayer = new MediaPlayer(_libVLC);
        }

        public void Play(string filePath)
        {
            Media media = new Media(_libVLC, new Uri(filePath));
            _mediaPlayer.Play(media);
        }

        public async Task PlayAsync(Track track)
        {
            if (track == null) throw new ArgumentNullException(nameof(track));

            using var media = new Media(_libVLC, new Uri(track.Source));
            await Task.Run(() => _mediaPlayer.Play(media));
            TrackChanged?.Invoke(track);
        }

        public void Pause()
        {
            _mediaPlayer.Pause();
        }

        public void Resume()
        {
            _mediaPlayer.SetPause(false);
        }

        public void Stop()
        {
            _mediaPlayer.Stop();
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
    }
}
