using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using majestic_player.core.Interfaces;
using majestic_player.core.Models;
using majestic_player.infrastructure.Models;
using ReactiveUI;


namespace majestic_player.infrastructure.Services
{
    public class PlaybackQueueService(IAudioService audioPlayer) : IPlaybackQueueService
    {
        private Track? _currentTrack;
        public Track? CurrentTrack
        {
            get => _currentTrack;
            private set
            {
                CurrentTrackChanged?.Invoke(value);
                _currentTrack = value;
            }
        }

        private int _currentIndex = -1;
        public int CurrentIndex
        {
            get => _currentIndex;
            set 
            {
                if (value == -1)
                {
                    CurrentTrack = null;
                    return;
                }

                _currentIndex = Math.Clamp(value, 0, _queue.Count);
                //Debug.WriteLine($"New QueueIndex: {_currentIndex}");
                //Debug.WriteLine($"Queue length: {_queue.Count}");
                CurrentTrack = _queue[_currentIndex];
            }
        }

        private readonly ObservableCollection<Track> _queue = new();
        public IEnumerable<Track> Queue => _queue;

        public event Action? QueueChanged;
        public event Action<Track?>? CurrentTrackChanged;

        public void AddTracksToQueue(IEnumerable<Track> tracks)
        {
            foreach (var track in tracks)
                _queue.Add(track);
        }

        public void CreateQueue(Track startTrack, IEnumerable<Track> tracks)
        {
            ClearQueue();

            _queue.Add(startTrack);
            AddTracksToQueue(tracks.Where(x => x != startTrack));
            CurrentIndex = 0;
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

            if (CurrentIndex >= 0 && CurrentTrack != null)
                CurrentIndex = _queue.IndexOf(CurrentTrack);

            QueueChanged?.Invoke();
        }

        public Track ToNextTrackInQueue()
        {
            if (CurrentIndex < _queue.Count - 1) CurrentIndex++;

            return CurrentTrack;
        }

        public Track ToPreviousTrackInQueue()
        {
            if (CurrentIndex > 0) CurrentIndex--;

            return CurrentTrack;
        }

        public void ClearQueue()
        {
            _queue.Clear();
            CurrentIndex = -1;
        }
    }
}