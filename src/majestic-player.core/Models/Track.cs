using System.Collections.Specialized;
using majestic_player.core.Enums;

namespace majestic_player.core.Models
{
    public class Track
    {
        private Guid _id = Guid.NewGuid();
        private string _title = "Empty Title";
        private string _artist = "Empty Artist";
        private string _album = "Empty Album";

        public Guid Id { get => _id; private set => _id = value; }
        public string? Title { get => _title; set => _title = value??"Empty Title"; }
        public string? Artist { get => _artist; set => _artist = value??"Empty Artist"; }
        public string? Album { get => _album; set => _album = value??"Empty Album"; }
        public TimeSpan Duration { get; set; }
        public string? Source { get; set; } // directory path OR url
        public SourceType SourceType { get; set; }
    }
}