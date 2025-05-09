using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using majestic_player.winui.ViewModels;
using majestic_player.winui.Views;
using Microsoft.UI.Xaml.Data;

namespace majestic_player.winui
{
    public class ViewModelToViewConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is SearchTabViewModel) return new SearchTabView();
            if (value is LibraryTabViewModel) return new LibraryTabView();
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
