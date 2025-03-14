using System.Collections.ObjectModel;
using majestic_player.core.Models;
using majestic_player.infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace majestic_player.ui.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly LibraryService _libraryService;

    public ObservableCollection<Track> AllTracks { get; } = new();

    public MainWindowViewModel()
    {
        //TODO: переделать инициальзацию
        // _libraryService = new LibraryService();

        // _libraryService = libraryService;
        // LoadTracks();

        var services = majestic_player.ui.Program.Services;

        if (services == null) return;

        using var serviceProvider = services.BuildServiceProvider();
        _libraryService = serviceProvider.GetService<LibraryService>();
        
        LoadTracks();
    }

    private async void LoadTracks()
    {
        var tracks = await _libraryService.GetAllTracksAsync();
        AllTracks.Clear();
        foreach (var track in tracks) AllTracks.Add(track);
    }
    // public List<Track> Tracks { get; };
}
