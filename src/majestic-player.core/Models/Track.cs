using majestic_player.core.Enums;
using CommunityToolkit.Mvvm.ComponentModel;

namespace majestic_player.core.Models
{
    public class Track : ObservableObject
    {
        private Guid _id = Guid.NewGuid();
        private string? _hash;
        private string _title = String.Empty;
        private string _artist = String.Empty;
        private string _album = String.Empty;
        private string _fileExtention = String.Empty;

        public Guid Id 
        { 
            get => _id; 
            set => SetProperty(ref _id, value); 
        }
        public required string? Hash
        {
            get => _hash??throw new Exception("Hash is null!");
            set => SetProperty(ref _hash, value??throw new Exception("New hash is null!"));
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
        private string? FileExtention
        {
            get => _fileExtention;
            set => SetProperty(ref _fileExtention, value??String.Empty);
        }
        
        public TimeSpan Duration { get; set; }
        public string? Source { get; set; } // directory path OR url
        public SourceType SourceType { get; set; }
    }
}