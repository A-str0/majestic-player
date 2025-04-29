using majestic_player.core.Models;
using DynamicData;

namespace majestic_player.core.Interfaces
{
    /// <summary>
    /// Interface for tracks indexation
    /// </summary>
    public interface IMediaHandlerService
    {
        public Task<IEnumerable<string>> GetAudioFilesAsync(string folderPath);
        public Track GetTrackMetadata(string filePath);
        public Task ScanFolderForAudioAsync(string folderPath);
        protected string ComputeFileHash(string filePath);

        public void AddFolder(string folderPath);
        public IObservable<IChangeSet<string>> Folders { get; }
    }
}