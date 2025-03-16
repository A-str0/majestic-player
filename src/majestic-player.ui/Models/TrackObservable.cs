using CommunityToolkit.Mvvm.ComponentModel;
using majestic_player.core.Models;

namespace majestic_player.ui.Models
{
    public class TrackObservable : ObservableObject
    {
        private string? _title;

        public string? Title 
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public TrackObservable(Track trackBase)
        {
            Title = trackBase.Title;

        }
    }
}