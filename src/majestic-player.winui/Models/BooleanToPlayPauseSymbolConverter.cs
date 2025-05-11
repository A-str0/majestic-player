using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using System;

namespace majestic_player.winui
{
    public class BooleanToPlayPauseSymbolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool isPlaying)
            {
                return isPlaying ? Symbol.Pause : Symbol.Play;
            }
            return Symbol.Play;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}