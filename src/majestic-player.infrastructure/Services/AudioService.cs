using System.Diagnostics;
using System.IO;
using System.Xml.Serialization;
using LibVLCSharp.Shared;
using majestic_player.core.Interfaces;
using majestic_player.core.Models;
using MonoTorrent;

namespace majestic_player.infrastructure.Models
{
    public class AudioService : IAudioService, IDisposable
    {
        private readonly LibVLC _libVLC;
        private readonly MediaPlayer _mediaPlayer;
        public bool IsPlaying { get => _mediaPlayer.IsPlaying; }

        public event Action<Track>? TrackChanged;
        public event Action? EndReached;
        public event Action<bool>? PlayingStateChanged;

        public AudioService()
        {
            Core.Initialize();
            
            _libVLC = new LibVLC();
            _mediaPlayer = new MediaPlayer(_libVLC);

            _mediaPlayer.EndReached += MediaPlayer_EndReached;
            _mediaPlayer.Paused += MediaPlayer_Paused;
            _mediaPlayer.Playing += MediaPayer_Playing;
        }

        private void MediaPlayer_Paused(object? sender, EventArgs e) => PlayingStateChanged?.Invoke(false);
        private void MediaPayer_Playing(object? sender, EventArgs e) => PlayingStateChanged?.Invoke(true);

        public async Task PlayAsync(Track track)
        {
            ArgumentNullException.ThrowIfNull(track);

            using var media = new Media(_libVLC, new Uri(track.Source));
            await Task.Run(() => _mediaPlayer.Play(media));
            
            TrackChanged?.Invoke(track);
        }

        public async Task PlayAsync(Track track, string uri)
        {
            ArgumentNullException.ThrowIfNull(track);

            using var media = new Media(_libVLC, uri, FromType.FromLocation);
            await Task.Run(() => _mediaPlayer.Play(media));

            TrackChanged?.Invoke(track);
        }

        public void PlayPause()
        {
            _mediaPlayer.SetPause(_mediaPlayer.IsPlaying);
        }

        public float GetCurrentPosition() => _mediaPlayer.Position;

        public void SetVolume(float volume)
        {
            _mediaPlayer.Volume = (int)volume;
        }

        public void SetPosition(float pos)
        {
            if (_mediaPlayer.Length <= 0) return; 
            
            _mediaPlayer.Position = Math.Clamp(pos, 0f, 1f); 
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
