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
        private TimeSpan _duration = TimeSpan.FromSeconds(0);
        private UInt16 _year = 0;
        private string? _source = String.Empty;
        private SourceType _sourceType;

        public Guid Id 
        { 
            get => _id; 
            set => SetProperty(ref _id, value); 
        }
        public string? Hash
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
        public TimeSpan? Duration 
        {
            get => _duration; 
            set => SetProperty(ref _duration, value??TimeSpan.FromSeconds(0)); 
        }
        public UInt16? Year
        {
            get => _year;
            set => SetProperty(ref _year, value??0);
        }

        public string? Source 
        { 
            get => _source; 
            set => SetProperty(ref _source, value); 
        }

        public SourceType SourceType 
        { 
            get => _sourceType; 
            set => SetProperty(ref _sourceType, value); 
        }
    }
}