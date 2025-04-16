using DynamicData;
using majestic_player.core.Interfaces;
using majestic_player.core.Models;
using majestic_player.infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace majestic_player.infrastructure.Services
{
    public class LibraryService : ILibraryService
    {
        private readonly AppDBContext _context;

        private readonly SourceCache<Track, Guid> _tracksCache = new(x => x.Id);
        public IObservable<IChangeSet<Track, Guid>> Tracks => _tracksCache.Connect();

        public LibraryService(AppDBContext context)
        {
            _context = context;

            LoadTracksAsync();
        }

        public async Task LoadTracksAsync()
        {
            var tracks = await GetAllTracksAsync();
            _tracksCache.Edit(updater => updater.AddOrUpdate(tracks));
        }

        public async Task<List<Track>> GetAllTracksAsync()
        {
            return await _context.Tracks.ToListAsync();
        }

        public async Task AddTrackAsync(Track track)
        {
            if (_context.Tracks.Any(t => track.Hash == t.Hash))
            {
                Console.WriteLine($"Track {track.Hash} is already in DB");
                return;
            }

            _context.Tracks.Add(track);
            await _context.SaveChangesAsync();
            Console.WriteLine($"Track {track.Title} added");
        }

        public async Task<bool> IsTrackExists(string? hash)
        {
            return await _context.Tracks.AnyAsync(t => t.Hash == hash);
        }
    }
}