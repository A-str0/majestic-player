using majestic_player.core.Interfaces;

namespace majestic_player.infrastructure.Providers
{
    public class LocalAudioSource : IAudioSource
    {
        public Stream AudioStream { get; set; }
        public Uri? StreamUri { get; set; }
        public TimeSpan Duration { get; set; }

        public LocalAudioSource(string filePath)
        {
            StreamUri = new Uri(filePath);
            AudioStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);

            Duration = TimeSpan.Zero; 
        }

        public void Dispose()
        {
            AudioStream?.Dispose();
        }
    }
}
