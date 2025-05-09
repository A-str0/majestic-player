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

        public AudioService()
        {
            Core.Initialize();
            
            _libVLC = new LibVLC();
            _mediaPlayer = new MediaPlayer(_libVLC);

            _mediaPlayer.EndReached += MediaPlayer_EndReached;
        }

        public async Task PlayAsync(Track track)
        {
            ArgumentNullException.ThrowIfNull(track);

            using var media = new Media(_libVLC, new Uri(track.Source));
            await Task.Run(() => _mediaPlayer.Play(media));
            
            TrackChanged?.Invoke(track);
        }

        public async Task PlayAsync(Stream stream)
        {
            ArgumentNullException.ThrowIfNull(stream);

            //using var media = new Media(_libVLC, new Uri(uri));

            StreamMediaInput mediaInput = new StreamMediaInput(stream);

            using var media = new Media(_libVLC, mediaInput);

            await Task.Run(() => _mediaPlayer.Play(media));
        }

        public async Task PlayAsync(string uri)
        {
            ArgumentNullException.ThrowIfNull(uri);

            using var media = new Media(_libVLC, uri, FromType.FromLocation);

            await Task.Run(() => _mediaPlayer.Play(media));
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
