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

        public bool IsPlaying { get => _mediaPlayer.IsPlaying; }

        public AudioPlayer()
        {
            Core.Initialize();
            
            _libVLC = new LibVLC();
            _mediaPlayer = new MediaPlayer(_libVLC);
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
            _mediaPlayer.SetPause(!_mediaPlayer.IsPlaying);
        }

        public void Next()
        {
            // TODO: implement next track logic
        }

        public void Previous()
        {
            // TODO: implement previous track logic
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
