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
    }
}