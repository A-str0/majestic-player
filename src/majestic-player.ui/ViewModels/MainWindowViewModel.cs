﻿﻿﻿using System.Collections.ObjectModel;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using majestic_player.core.Models;
using majestic_player.infrastructure.Services;
using System;

namespace majestic_player.ui.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly LibraryService? _libraryService; // Allow null

    public ObservableCollection<Track> AllTracks { get; } = new();

    public MainWindowViewModel()
    {
        Console.WriteLine("Started");
    
        IServiceProvider serviceProvider = Program.Services.CreateScope().ServiceProvider;

        _libraryService = serviceProvider.GetRequiredService<LibraryService>();

        LoadTracks();
    }

    private async void LoadTracks()
    {
        Console.WriteLine("Loading tracks...");
        
        List<Track> tracks = await _libraryService.GetAllTracksAsync();

        // TODO: Delete this TESTING
        // List<Track> tracks = new List<Track>();
        for (int i = 0; i < 10; i++)
        {
            Track track = new Track() {Title = "Track " + i.ToString()};
            tracks.Add(track);
        }

        AllTracks.Clear();
        foreach (var track in tracks) AllTracks.Add(track);

        Console.WriteLine("Tracks loaded");
    }
}
