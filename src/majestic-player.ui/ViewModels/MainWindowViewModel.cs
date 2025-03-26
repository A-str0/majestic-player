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

namespace majestic_player.ui.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly LibraryService? _libraryService;
    private readonly FileHandlerService? _fileScannerService;
    private readonly IStorageProvider? _storageProvider;
    private readonly AudioPlayer? _audioPlayer;

    public ObservableCollection<Track> AllTracks { get; } = new();
    public ObservableCollection<string> SelectedFolders { get; } = new();

    public ICommand AddFolderCommand { get; private set; }
    public ReactiveCommand<Track, Unit> PlayTrackCommand { get; private set; }
    public ICommand PlayPauseCommand { get; private set; }
    public ICommand NextCommand { get; private set; }
    public ICommand PreviousCommand { get; private set; }

    // TODO: убрать этот способ
    private async void TestSetup() { await ScanFolderForAudio("C:/Users/magesty_/Downloads/"); }

    public MainWindowViewModel(IStorageProvider storageProvider)
    {
        Console.WriteLine("Started");

        // SetupCommands
        AddFolderCommand = ReactiveCommand.Create(AddFolderDialog);
        PlayTrackCommand = ReactiveCommand.Create<Track>(PlayTrack);

        PlayPauseCommand = ReactiveCommand.Create(PlayPause);
        NextCommand = ReactiveCommand.Create(Next);
        PreviousCommand = ReactiveCommand.Create(Previous);
    
        IServiceProvider serviceProvider = Program.Services.CreateScope().ServiceProvider;

        _libraryService = serviceProvider.GetRequiredService<LibraryService>();
        _fileScannerService = serviceProvider.GetRequiredService<FileHandlerService>();
        _storageProvider = storageProvider;

        _audioPlayer = serviceProvider.GetRequiredService<AudioPlayer>();


        // TODO: убрать этот способ
        TestSetup();

        LoadTracks();
    }

    private async void LoadTracks()
    {
        Console.WriteLine("Loading tracks...");
        
        List<Track> tracks = await _libraryService.GetAllTracksAsync();
        AllTracks.Clear();
        foreach (var track in tracks) AllTracks.Add(track);

        Console.WriteLine("Tracks loaded");
    }

    public void Test(object msg)
    {
        Console.WriteLine($"Test {msg}");
    }

    public async void PlayTrack(Track track) => await _audioPlayer?.PlayAsync(track);
    public void PlayPause() => _audioPlayer?.PlayPause();
    public void Next() => _audioPlayer?.Next();
    public void Previous() => _audioPlayer?.Previous();

    public async void AddFolderDialog()
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
                SelectedFolders.Add(path);
                await ScanFolderForAudio(path);
            }
        }
    }

    private async Task ScanFolderForAudio(string folderPath)
    {
        if (_fileScannerService == null)
        {
            Console.WriteLine($"FileScannerService is null");
            return;
        }

        foreach (var file in _fileScannerService.GetAudioFiles(folderPath))
        {
            var track = _fileScannerService.GetTrackMetadata(file);
            await _libraryService?.AddTrackAsync(track);
        }

        LoadTracks();
    }
}
