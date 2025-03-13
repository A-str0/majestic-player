using Microsoft.EntityFrameworkCore;
using majestic_player.core.Models;

namespace majestic_player.infrastructure.Models
{
    public class AppDBContext : DbContext
    {
        public DbSet<Track> Tracks { get; set; }
        public DbSet<Playlist> Playlists { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            // TODO: разделить для разных ОС
            string dbPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "MajesticPlayer",
                "database.db"
            );
            options.UseSqlite($"Data Source={dbPath}");
        }
    }
}