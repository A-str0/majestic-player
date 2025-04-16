using System;
using System.Windows.Input;
using System.Reactive;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Avalonia.Platform.Storage;
using majestic_player.core.Models;
using majestic_player.infrastructure.Services;
using majestic_player.infrastructure.Models;
using ReactiveUI;
using System.Linq;
using System.Reactive.Linq;
using DynamicData;
using DynamicData.Binding;
using majestic_player.core.Interfaces;
using Avalonia.Controls;

namespace majestic_player.ui.ViewModels;

public partial class MainWindowViewModel : ReactiveObject
{
    #region Services
    private readonly LibraryService? _libraryService;
    private readonly IMediaHandlerService? _mediaHandlerService;
    private readonly IStorageProvider? _storageProvider;
    private readonly PlaybackQueueService? _playbackQueueService;
    private readonly IAudioService? _audioService;
    #endregion

    private ReadOnlyObservableCollection<Track> _allTracks;
    public ReadOnlyObservableCollection<Track> AllTracks => _allTracks;
    private ReadOnlyObservableCollection<string> _mediaFolders;
    public ReadOnlyObservableCollection<string> MediaFolders => _mediaFolders;
    private Track CurrentTrack => _playbackQueueService.CurrentTrack;

    #region Commands
    public ReactiveCommand<Unit, Unit> AddFolderCommand { get; private set; }
    public ReactiveCommand<Track, Unit> PlayTrackCommand { get; private set; }
    public ICommand PlayPauseCommand { get; private set; }
    public ICommand NextCommand { get; private set; }
    public ICommand PreviousCommand { get; private set; }
    #endregion

    public MainWindowViewModel(IStorageProvider storageProvider)
    {
        Console.WriteLine("Started");

        // Setup Commands
        AddFolderCommand = ReactiveCommand.CreateFromTask(AddFolderDialog);
        PlayTrackCommand = ReactiveCommand.CreateFromTask<Track>(PlayTrack);

        PlayPauseCommand = ReactiveCommand.Create(PlayPause);
        NextCommand = ReactiveCommand.Create(PlayNextTrackInQueue);
        PreviousCommand = ReactiveCommand.Create(PlayPreviousTrackInQueue);

        // Setup Services
        IServiceProvider serviceProvider = Program.Services.CreateScope().ServiceProvider;

        _audioService = serviceProvider.GetRequiredService<IAudioService>();

        _libraryService = serviceProvider.GetRequiredService<LibraryService>();
        _mediaHandlerService = serviceProvider.GetRequiredService<IMediaHandlerService>();
        _storageProvider = storageProvider;
        _playbackQueueService = serviceProvider.GetRequiredService<PlaybackQueueService>();

        // Setup observers
        LoadFolders();
        LoadTracks();
    }

    private async Task LoadFolders()
    {
        Console.WriteLine("Loading folders...");

        _mediaHandlerService.Folders
            .ObserveOn(RxApp.MainThreadScheduler)
            .Bind(out _mediaFolders)
            .Do(_ => UpdateTracksAsync().GetAwaiter().GetResult())
            .Subscribe();

        Console.WriteLine("Folders loaded");
    }

    private async Task LoadTracks()
    {
        Console.WriteLine("Loading tracks...");

        await _libraryService?.LoadTracksAsync();

        _libraryService?.Tracks
            .ObserveOn(RxApp.MainThreadScheduler)
            .Bind(out _allTracks)
            .Subscribe();

        Console.WriteLine("Tracks loaded");
    }

    public async Task PlayTrack(Track track)
    {
        if (_playbackQueueService?.Queue.Count() == 0)
        {
            _playbackQueueService.CreateQueue(track, AllTracks);
        }

        await _audioService?.PlayAsync(track);
    }
    
    public async Task PlayPause() =>  _audioService?.PlayPause();
    public async Task PlayNextTrackInQueue()
    {
        Track nextTrack = _playbackQueueService?.ToNextTrackInQueue();
        await _audioService?.PlayAsync(nextTrack);
    }
    
    public async Task PlayPreviousTrackInQueue()
    {
        Track prevTrack = _playbackQueueService?.ToPreviousTrackInQueue();
        await _audioService?.PlayAsync(prevTrack);
    }

    private async Task UpdateTracksAsync()
    {
        Console.WriteLine("Updating tracks due to folder change...");
        await _libraryService.LoadTracksAsync();
        Console.WriteLine("Tracks updated");
    }

    public async Task AddFolderDialog()
    {
        var folders = await _storageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            AllowMultiple = true,
            Title = "Select directories with music"
        });

        foreach (var folder in folders)
        {
            if (folder.TryGetLocalPath() is { } path)
            {
                _mediaHandlerService?.AddFolder(path);
                await _mediaHandlerService?.ScanFolderForAudio(path);
            }
        }
    }
}
