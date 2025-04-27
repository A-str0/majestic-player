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

namespace majestic_player.winui.ViewModels;

public partial class MainWindowViewModel : ReactiveObject
{
    #region Services
    private readonly LibraryService? _libraryService;
    private readonly IMediaHandlerService? _mediaHandlerService;
    private readonly PlaybackQueueService? _playbackQueueService;
    private readonly IAudioService? _audioService;
    #endregion

    private ReadOnlyObservableCollection<Track> _allTracks;
    public ReadOnlyObservableCollection<Track> AllTracks => _allTracks;
    private ReadOnlyObservableCollection<string> _mediaFolders;
    public ReadOnlyObservableCollection<string> MediaFolders => _mediaFolders;
    public Track? CurrentTrack => _playbackQueueService?.CurrentTrack;

    #region Commands
    public ReactiveCommand<Unit, Unit> AddFolderCommand { get; private set; }
    public ReactiveCommand<Track?, Unit> PlayTrackCommand { get; private set; }
    public ICommand PlayPauseCommand { get; private set; }
    public ICommand NextCommand { get; private set; }
    public ICommand PreviousCommand { get; private set; }
    #endregion

    public MainWindowViewModel()
    {
        Console.WriteLine("Started");

        // Setup Commands
        AddFolderCommand = ReactiveCommand.CreateFromTask(AddFolderDialogAsync);
        PlayTrackCommand = ReactiveCommand.CreateFromTask<Track?>(PlayTrack);

        PlayPauseCommand = ReactiveCommand.Create(PlayPause);
        NextCommand = ReactiveCommand.Create(PlayNextTrackInQueue);
        PreviousCommand = ReactiveCommand.Create(PlayPreviousTrackInQueue);

        // Setup Services
        IServiceProvider serviceProvider = App.Services.CreateScope().ServiceProvider;

        _audioService = serviceProvider.GetRequiredService<IAudioService>();

        _libraryService = serviceProvider.GetRequiredService<LibraryService>();
        _mediaHandlerService = serviceProvider.GetRequiredService<IMediaHandlerService>();
        _playbackQueueService = serviceProvider.GetRequiredService<PlaybackQueueService>();

        // Setup observers
        LoadFolders();
        LoadTracks();
    }

    private async void LoadFolders()
    {
        Console.WriteLine("Loading folders...");

        Console.WriteLine("Folders loaded");
    }

    private async void LoadTracks()
    {
        Console.WriteLine("Loading tracks...");

        _libraryService?.LoadTracksAsync();

        _libraryService?.Tracks
            .ObserveOn(RxApp.MainThreadScheduler)
            .Bind(out _allTracks)
            .Do(_ => UpdateTracksAsync()?.GetAwaiter().GetResult())
            .Subscribe();

        Console.WriteLine("Tracks loaded");
    }

    public async Task? PlayTrack(Track track)
    {
        Debug.WriteLine($"Track {track.Title} is playing");

        if (_playbackQueueService?.Queue.Count() == 0)
        {
            Debug.WriteLine($"Queue was created");

            _playbackQueueService.CreateQueue(track, AllTracks);
        }

        await _audioService?.PlayAsync(track);
    }

    public void PlayPause() => _audioService?.PlayPause();

    public void PlayNextTrackInQueue()
    {
        Track? nextTrack = _playbackQueueService?.ToNextTrackInQueue();

        Debug.WriteLine($"Playing next track in queue: {nextTrack?.Title}");

        _audioService?.PlayAsync(nextTrack);
    }

    public void PlayPreviousTrackInQueue()
    {
        Track? prevTrack = _playbackQueueService?.ToPreviousTrackInQueue();
        _audioService?.PlayAsync(prevTrack);
    }

    public async Task AddFolderDialogAsync()
    {
        System.Diagnostics.Debug.WriteLine($"Add folder dialog appeared");

        var folderPicker = new FolderPicker
        {
            ViewMode = PickerViewMode.List
        };
        folderPicker.FileTypeFilter.Add("*");

        IntPtr hwnd = WindowNative.GetWindowHandle(App.CurrentWindow);
        InitializeWithWindow.Initialize(folderPicker, hwnd);

        var folder = await folderPicker.PickSingleFolderAsync();

        if (folder != null)
        {
            string path = folder.Path;
            if (!string.IsNullOrEmpty(path))
            {
                await _mediaHandlerService?.AddFolder(path);
                await _mediaHandlerService?.ScanFolderForAudio(path);
            }
        }
    }

    private async Task? UpdateTracksAsync()
    {
        Console.WriteLine("Updating tracks due to folder change...");
        _libraryService?.LoadTracksAsync();
        Console.WriteLine("Tracks updated");
    }
}
