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

    private async Task? LoadFolders()
    {
        Console.WriteLine("Loading folders...");

        _mediaHandlerService?.Folders
            .ObserveOn(RxApp.MainThreadScheduler)
            .Bind(out _mediaFolders)
            .Do(_ => UpdateTracksAsync()?.GetAwaiter().GetResult())
            .Subscribe();

        Console.WriteLine("Folders loaded");
    }

    private async Task? LoadTracks()
    {
        Console.WriteLine("Loading tracks...");

        await _libraryService?.LoadTracksAsync();

        _libraryService?.Tracks
            .ObserveOn(RxApp.MainThreadScheduler)
            .Bind(out _allTracks)
            .Subscribe();

        Console.WriteLine("Tracks loaded");
    }

    public async Task? PlayTrack(Track track)
    {
        if (_playbackQueueService?.Queue.Count() == 0)
        {
            _playbackQueueService.CreateQueue(track, AllTracks);
        }

        await _audioService?.PlayAsync(track);
    }

    public async Task? PlayPause() => _audioService?.PlayPause();

    public async Task? PlayNextTrackInQueue()
    {
        Track? nextTrack = _playbackQueueService?.ToNextTrackInQueue();
        await _audioService?.PlayAsync(nextTrack);
    }

    public async Task? PlayPreviousTrackInQueue()
    {
        Track? prevTrack = _playbackQueueService?.ToPreviousTrackInQueue();
        await _audioService?.PlayAsync(prevTrack);
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
                _mediaHandlerService?.AddFolder(path);
                await _mediaHandlerService?.ScanFolderForAudio(path);
            }
        }
    }

    private async Task? UpdateTracksAsync()
    {
        Console.WriteLine("Updating tracks due to folder change...");
        await _libraryService?.LoadTracksAsync();
        Console.WriteLine("Tracks updated");
    }
}
