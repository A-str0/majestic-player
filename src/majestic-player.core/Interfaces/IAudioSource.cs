namespace majestic_player.core.Interfaces
{
    public interface IAudioSource : IDisposable
    {
        public Stream AudioStream { get; protected set; }    
        public Uri? StreamUri { get; protected set; }
        public TimeSpan Duration { get; protected set; }
    }
}