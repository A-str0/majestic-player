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
using System.Diagnostics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Controls;

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

    private Track? _currentTrack;
    public Track? CurrentTrack
    {
        get => _currentTrack;
        set => this.RaiseAndSetIfChanged(ref _currentTrack, value);
    }

    private bool _isPlaying;
    public bool IsPlaying
    {
        get => _isPlaying;
        set => this.RaiseAndSetIfChanged(ref _isPlaying, value);
    }

    private DispatcherTimer _timer;

    private TimeSpan _positionValue;
    public TimeSpan PositionValue
    {
        get => _positionValue;
        set => this.RaiseAndSetIfChanged(ref _positionValue, value);
    }

    private float _progressValue;
    public float ProgressValue
    {
        get => _progressValue;
        set => this.RaiseAndSetIfChanged(ref _progressValue, value);
    }

    private readonly SearchTabViewModel _searchTabViewModel;
    private readonly LibraryTabViewModel _libraryTabViewModel;
    private object _currentViewModel;
    public object CurrentViewModel
    {
        get => _currentViewModel;
        set => this.RaiseAndSetIfChanged(ref _currentViewModel, value);
    }

    private readonly DispatcherQueue _dispatcherQueue;

    #region Commands
    public ReactiveCommand<int, Unit> SelectTabCommand { get; private set; }

    public ReactiveCommand<Unit, Unit> PlayPauseCommand { get; private set; }
    public ReactiveCommand<Unit, Unit> NextCommand { get; private set; }
    public ReactiveCommand<Unit, Unit> PreviousCommand { get; private set; }

    public ReactiveCommand<Track, Unit> AddTrackToLibraryCommand { get; private set; }

    public ReactiveCommand<float, Unit> SetTrackPositionCommand { get; private set; }
    #endregion

    public MainWindowViewModel()
    {
        Console.WriteLine("Started");

        _dispatcherQueue = DispatcherQueue.GetForCurrentThread();

        // Setup Commands
        SelectTabCommand = ReactiveCommand.CreateFromTask<int>(SelectTab);

        PlayPauseCommand = ReactiveCommand.Create(PlayPause);
        NextCommand = ReactiveCommand.CreateFromTask(PlayNextTrackInQueue);
        PreviousCommand = ReactiveCommand.CreateFromTask(PlayPreviousTrackInQueue);

        AddTrackToLibraryCommand = ReactiveCommand.CreateFromTask<Track>(AddTrackToLibrary);

        SetTrackPositionCommand = ReactiveCommand.Create<float>(SetTrackPosition);

        // Setup Services
        IServiceProvider serviceProvider = App.Services.CreateScope().ServiceProvider;

        _audioService = serviceProvider.GetRequiredService<IAudioService>();

        _libraryService = serviceProvider.GetRequiredService<LibraryService>();
        _playbackQueueService = serviceProvider.GetRequiredService<PlaybackQueueService>();

        _playbackQueueService.CurrentTrackChanged += PlaybackQueueService_CurrentTrackChanged;
        _audioService.TrackChanged += PlaybackQueueService_CurrentTrackChanged;

        _audioService.EndReached += PlaybackQueueService_EndReached;
        _audioService.PlayingStateChanged += AudioService_PlayingStateChanged;


        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(500)
        };
        _timer.Tick += Timer_Tick;

        IsPlaying = false;
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

    public void SetTrackPosition(float value) => _audioService.SetPosition(value);

    public async Task AddTrackToLibrary(Track track)
    {
        if (track == null)
            return;

        Debug.WriteLine($"Like for {track.Title}");

        await _libraryService.AddTrackAsync(track);
    }

    private void AudioService_PlayingStateChanged(bool obj)
    {
        _dispatcherQueue.TryEnqueue(() =>
        {
            IsPlaying = obj;

            if (IsPlaying)
                _timer.Start();
            else
                _timer.Stop();
        });
    }

    private void PlaybackQueueService_CurrentTrackChanged(Track? track)
    {
        if (track == null)
            return;

        Debug.WriteLine($"CurrentTrack changed: {track}");

        CurrentTrack = track;
    }

    private void PlaybackQueueService_EndReached()
    {
        _dispatcherQueue.TryEnqueue(async () =>
        {
            await PlayNextTrackInQueue();
        });
    }

    private void Timer_Tick(object sender, object e)
    {
        if (CurrentTrack == null) return;

        ProgressValue = _audioService.GetCurrentPosition();
        PositionValue = (TimeSpan)(_progressValue * CurrentTrack.Duration);
    }
}
