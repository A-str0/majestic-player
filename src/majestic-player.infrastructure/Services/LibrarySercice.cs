using majestic_player.core.Models;
using majestic_player.infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace majestic_player.infrastructure.Services
{
    public class LibraryService
    {
        private readonly AppDBContext _context;

        public LibraryService(AppDBContext context)
        {
            _context = context;
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