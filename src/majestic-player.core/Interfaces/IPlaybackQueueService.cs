using majestic_player.core.Models;
using DynamicData;

namespace majestic_player.core.Interfaces
{
    /// <summary>
    /// Interface for playback queue contols
    /// </summary>
    public interface IPlaybackQueueService
    {
        public IEnumerable<Track> Queue { get; }

        public event Action? QueueChanged;

        public void AddTracksToQueue(IEnumerable<Track> tracks);
        public void CreateQueue(Track startTrack, IEnumerable<Track> tracks);
        public void Shuffle();
        public Track ToNextTrackInQueue();
        public Track ToPreviousTrackInQueue();
        public void ClearQueue();
    }
}