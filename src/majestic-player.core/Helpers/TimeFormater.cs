using System;

namespace majestic_player.core.Helpers
{
    public static class TimeFormatter
    {
        public static string ToDisplayString(TimeSpan time)
        {
            return $"{(int)time.TotalMinutes:00}:{time.Seconds:00}";
        }
    }
}