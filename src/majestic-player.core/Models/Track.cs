using majestic_player.core.Enums;
using CommunityToolkit.Mvvm.ComponentModel;

namespace majestic_player.core.Models
{
    public class Track : ObservableObject
    {
        private Guid _id = Guid.NewGuid();
        private string _title = String.Empty;
        private string _artist = String.Empty;
        private string _album = String.Empty;
        
        public Guid Id 
        { 
            get => _id; 
            private set => SetProperty(ref _id, value); 
        }
        public string? Title 
        { 
            get => _title; 
            set => SetProperty(ref _title, value??String.Empty); 
        }
        public string? Artist 
        { 
            get => _artist; 
            set => SetProperty(ref _artist, value??String.Empty); 
        }
        public string? Album 
        { 
            get => _album; 
            set => SetProperty(ref _album, value??String.Empty);
        }
        
        public TimeSpan Duration { get; set; }
        public string? Source { get; set; } // directory path OR url
        public SourceType SourceType { get; set; }
    }
}