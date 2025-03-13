using majestic_player.core.Models;
using majestic_player.infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace majestic_player.infrastructure.Services
{
    public class PlaylistService 
    {
        private readonly AppDBContext _context;

        public PlaylistService(AppDBContext context)
        {
            _context = context;
        }

        public async Task CreatePlaylist(string name)
        {
            if (await _context.Playlists.AnyAsync(p => p.Name == name))
                throw new Exception("Playlist with name " + name + " already exists");

            Playlist playlist = new Playlist { Name = name };
            _context.Playlists.Add(playlist);
            await _context.SaveChangesAsync();
        }

        public async Task AddTrack(Guid playlistId, Track track)
        {
            var playlist = await _context.Playlists.FindAsync(playlistId);
            if (playlist == null)
                throw new Exception("Playlist not found");

            playlist.Tracks.Add(track);
            await _context.SaveChangesAsync();
        }
    }
}
