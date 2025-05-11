using majestic_player.core.Models;
using DynamicData;
using System.Text.Json;

namespace majestic_player.core.Interfaces
{
    /// <summary>
    /// Interface for tracks indexation
    /// </summary>
    public interface IMediaHandlerService
    {
        public Task<IEnumerable<string>> GetLocalAudioFilesAsync(string folderPath);

        public Track GetTrackMetadataLocal(string filePath);
        public Task<Track> GetTrackMetadataTorrent(object manager, object file, string magnetLink);

        public Task ScanFolderForAudioAsync(string folderPath);

        protected string ComputeFileHash(string filePath);
        protected string ComputeStreamHash(Stream stream);

        public void AddFolder(string folderPath);
        public IObservable<IChangeSet<string>> Folders { get; }
    }
}