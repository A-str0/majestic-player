namespace majestic_player.core.Models
{
    public class Playlist
    {
        public Guid Id { get; } = Guid.NewGuid();
        public required string Title { get; set; }
        public List<Track> Tracks { get; } = new();
    }
}