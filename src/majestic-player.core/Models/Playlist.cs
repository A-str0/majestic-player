using CommunityToolkit.Mvvm.ComponentModel;

namespace majestic_player.core.Models
{
    public class Playlist : ObservableObject
    {
        private Guid _id = Guid.NewGuid();
        private string _name = String.Empty;
        private List<Track> _tracks = new List<Track>();

        public Guid Id 
        { 
            get => _id; 
            set => SetProperty(ref _id, value); 
        }
        public string? Name 
        { 
            get => _name;
            set => SetProperty(ref _name, value??String.Empty); 
        }
        public List<Track> Tracks 
        { 
            get => _tracks; 
            set => SetProperty(ref _tracks, value); 
        }
    }
}
