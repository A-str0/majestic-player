using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using DynamicData;
using majestic_player.core.Interfaces;
using majestic_player.core.Models;
using majestic_player.infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace majestic_player.infrastructure.Services
{
    public class LibraryService : ILibraryService
    {
        private IDbContextFactory<AppDBContext> _contextFactory;

        private readonly SourceCache<Track, Guid> _tracksCache = new(x => x.Id);
        public IObservable<IChangeSet<Track, Guid>> Tracks => _tracksCache.Connect();

        public event Action<Track>? TrackAdded;
        public LibraryService(IDbContextFactory<AppDBContext> contextFactory)
        {
            _contextFactory = contextFactory;

            //Task.Run(async () => await LoadTracksAsync());
        }

        /// <summary>
        /// THIS METHOD MUST BE CALLED IN UI THREAD
        /// Method for reloading all tracks from DB
        /// </summary>
        public async Task LoadTracksAsync()
        {
            Debug.WriteLine("Loading tracks");

            List<Track> tracks = await GetAllTracksAsync();

            _tracksCache.Edit(innerCache => innerCache.AddOrUpdate(tracks));
        }

        public async Task<List<Track>> GetAllTracksAsync()
        {
            Debug.WriteLine("Returning all tracks");

            using var context = _contextFactory.CreateDbContext();
            return await context.Tracks.ToListAsync();
        }

        public async Task AddTracksAsync(IEnumerable<Track> tracks)
        {
            Debug.WriteLine($"Tracks count: {tracks.Count()}");

            using var context = _contextFactory.CreateDbContext();
            foreach (var track in tracks)
            {
                if (await IsTrackExists(track.Hash))
                {
                    Debug.WriteLine($"Track {track.Hash} is already in DB");
                    continue;
                }

                context.Tracks.Add(track);
                Debug.WriteLine($"Track {track.Title} added");
            }

            var result = await context.SaveChangesAsync();
            Debug.WriteLine($"Saved {result} tracks to the database.");
        }

        public async Task AddTrackAsync(Track track)
        {
            using var context = _contextFactory.CreateDbContext();
            if (await IsTrackExists(track.Hash))
            {
                Console.WriteLine($"Track {track.Hash} is already in DB");
                return;
            }

            context.Tracks.Add(track);
            Console.WriteLine($"Track {track.Title} added");

            var result = await context.SaveChangesAsync();
            Debug.WriteLine($"Saved {result} tracks to the database.");

            TrackAdded?.Invoke( track );
        }

        public async Task<bool> IsTrackExists(string? hash)
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.Tracks.AnyAsync(t => t.Hash == hash);
        }
    }
}