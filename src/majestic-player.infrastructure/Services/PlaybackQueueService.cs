using System.Collections.ObjectModel;
using System.Linq;
using majestic_player.core.Models;
using majestic_player.infrastructure.Models;


namespace majestic_player.infrastructure.Services
{
    public class PlaybackQueueService
    {
        private readonly AudioPlayer _audioPlayer;
        private int _currentIndex = -1;

        private readonly ObservableCollection<Track> _queue = new();
        public IEnumerable<Track> Queue => _queue;

        public Action? QueueChanged;

        public Track? CurrentTrack 
        { 
            get => _currentIndex >= 0 ? _queue[_currentIndex] : null;
            set => _queue[_currentIndex] = value;
        }

        public PlaybackQueueService(AudioPlayer audioPlayer)
        {
            _audioPlayer = audioPlayer;

            _audioPlayer.EndReached += AudioPlayer_EndReached;
        }

        public void AddTracksToQueue(IEnumerable<Track> tracks)
        {
            foreach (var track in tracks)
            {
                _queue.Add(track);
            }
        }

        public void CreateQueue(Track startTrack, IEnumerable<Track> tracks)
        {
            ClearQueue();
            
            _queue.Add(startTrack);
            AddTracksToQueue(tracks);
        }

        public void Shuffle()
        {
            if (_queue.Count == 0) return;

            var rng = new Random();
            int count = _queue.Count;

            for (int i = 0; i < count; i++)
            {
                int j = rng.Next(i, count);
                if (i != j)
                    _queue.Move(j, i);
            }

            if (_currentIndex >= 0 && CurrentTrack != null)
                _currentIndex = _queue.IndexOf(CurrentTrack);

            QueueChanged?.Invoke();
        }

        public Track ToNextTrackInQueue()
        {
            if (_currentIndex < _queue.Count - 1) _currentIndex++;

            return CurrentTrack;
        }

        public Track ToPreviousTrackInQueue()
        {
            if (_currentIndex > 0) _currentIndex--;

            return CurrentTrack;
        }

        public void ClearQueue()
        {
            _queue.Clear();
            _currentIndex = -1;
        }

        public async void AudioPlayer_EndReached()
        {
            Track nextTrack = ToNextTrackInQueue();
            await _audioPlayer.PlayAsync(nextTrack);
        }
    }
}