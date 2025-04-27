using majestic_player.core.Models;
using DynamicData;

namespace majestic_player.core.Interfaces
{
    /// <summary>
    /// Interface for library control
    /// </summary>
    public interface ILibraryService
    {
        public Task? LoadTracksAsync();
        public Task<List<Track>>? GetAllTracksAsync();
        public Task? AddTracksAsync(IEnumerable<Track> tracks);
        public Task? AddTrackAsync(Track track);
        public Task<bool>? IsTrackExists(string? hash);

        public IObservable<IChangeSet<Track, Guid>> Tracks { get; }
    }
}