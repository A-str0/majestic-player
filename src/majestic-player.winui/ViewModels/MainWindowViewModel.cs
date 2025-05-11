using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using majestic_player.core.Interfaces;
using majestic_player.infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Windows.Input;
using Windows.Storage;
using ReactiveUI;
using majestic_player.core.Models;
using System.Reactive.Linq;
using DynamicData;
using Windows.Storage.Pickers;
using WinRT.Interop;
using System.Windows;
using System.Diagnostics;
using DynamicData.Binding;
using DynamicData.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;

namespace majestic_player.winui.ViewModels;

public partial class MainWindowViewModel : ReactiveObject
{
    #region Services
    private readonly LibraryService _libraryService;
    private readonly IMediaHandlerService _mediaHandlerService;
    private readonly PlaybackQueueService _playbackQueueService;
    private readonly IAudioService _audioService;
    private readonly TorrentSearchService _searchService;
    #endregion

    public Track? CurrentTrack => _playbackQueueService?.CurrentTrack;

    private readonly SearchTabViewModel _searchTabViewModel;
    private readonly LibraryTabViewModel _libraryTabViewModel;
    private object _currentViewModel;
    public object CurrentViewModel
    {
        get => _currentViewModel;
        set => this.RaiseAndSetIfChanged(ref _currentViewModel, value);
    }

    #region Commands
    public ReactiveCommand<int, Unit> SelectTabCommand { get; private set; }

    public ICommand PlayPauseCommand { get; private set; }
    public ICommand NextCommand { get; private set; }
    public ICommand PreviousCommand { get; private set; }
    #endregion

    public MainWindowViewModel()
    {
        Console.WriteLine("Started");

        // Setup Commands
        SelectTabCommand = ReactiveCommand.CreateFromTask<int>(SelectTab);

        PlayPauseCommand = ReactiveCommand.Create(PlayPause);
        NextCommand = ReactiveCommand.Create(PlayNextTrackInQueue);
        PreviousCommand = ReactiveCommand.Create(PlayPreviousTrackInQueue);

        // Setup Services
        IServiceProvider serviceProvider = App.Services.CreateScope().ServiceProvider;

        _audioService = serviceProvider.GetRequiredService<IAudioService>();

        _libraryService = serviceProvider.GetRequiredService<LibraryService>();
        _playbackQueueService = serviceProvider.GetRequiredService<PlaybackQueueService>();

    }

    public void PlayPause() => _audioService?.PlayPause();

    public async Task PlayNextTrackInQueue()
    {
        Track nextTrack = _playbackQueueService.ToNextTrackInQueue();

        if (nextTrack == null)
        {
            Debug.WriteLine("The next track in queue is null");
            return;
        }

        Debug.WriteLine($"Playing next track in queue: {nextTrack?.Title}");

        await _audioService.PlayAsync(nextTrack);
    }

    public async Task PlayPreviousTrackInQueue()
    {
        Track? prevTrack = _playbackQueueService?.ToPreviousTrackInQueue();

        if (prevTrack == null)
        {
            Debug.WriteLine("The previous track in queue is null");
            return;
        }

        await _audioService.PlayAsync(prevTrack);
    }

    public async Task SelectTab(int tabIndex)
    {
        CurrentViewModel = tabIndex switch
        {
            0 => _searchTabViewModel,
            1 => _libraryTabViewModel,
            _ => throw new ArgumentOutOfRangeException(nameof(tabIndex))
        };
    }
}
