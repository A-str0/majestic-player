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
        public Task<Track> GetTrackMetadataStream(Stream stream, string filePath, string source);

        public Task ScanFolderForAudioAsync(string folderPath);
        public Task ScanStreamsForAudio(List<Stream> streams, string filePath, string magnetLink);

        protected string ComputeFileHash(string filePath);
        protected string ComputeStreamHash(Stream stream);

        public void AddFolder(string folderPath);
        public IObservable<IChangeSet<string>> Folders { get; }
    }
}