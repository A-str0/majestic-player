using majestic_player.core.Models;
using DynamicData;

namespace majestic_player.core.Interfaces
{
    /// <summary>
    /// Interface for tracks indexation
    /// </summary>
    public interface IMediaHandlerService
    {
        public IEnumerable<string> GetAudioFiles(string folderPath);
        public Track GetTrackMetadata(string filePath);
        public Task ScanFolderForAudio(string folderPath);
        protected string ComputeFileHash(string filePath);

        public Task AddFolder(string folderPath);
        public IObservable<IChangeSet<string>> Folders { get; }
    }
}