namespace majestic_player.core.Models
{
public class Playlist
{
    public Guid Id { get; } = Guid.NewGuid();
    public required string Name { get; set; }
    public List<Track> Tracks { get; set; } = new List<Track>(); // Added property to hold tracks
}

}
