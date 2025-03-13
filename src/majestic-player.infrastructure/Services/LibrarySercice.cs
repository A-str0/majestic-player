using majestic_player.core.Models;
using majestic_player.infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace majestic_player.infrastructure.Services
{
    public class TrackLibraryService
    {
        private readonly AppDBContext _context;

        public TrackLibraryService(AppDBContext context)
        {
            _context = context;
        }

        public async Task<List<Track>> GetAllTracksAsync()
        {
            return await _context.Tracks.ToListAsync();
        }

        // public async Task<List<Track>> GetTracksByGenreAsync(string genre)
        // {
        //     return await _context.Tracks
        //         .Where(t => t.Genre == genre)
        //         .ToListAsync();
        // }

        // public async Task<List<Track>> GetTracksSortedByDateAsync()
        // {
        //     return await _context.Tracks
        //         .OrderByDescending(t => t.DateAdded)
        //         .ToListAsync();
        // }
    }
}