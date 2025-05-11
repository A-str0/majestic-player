using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Concurrency;
using System.Runtime.InteropServices.WindowsRuntime;
using CommunityToolkit.Mvvm.Messaging;
using majestic_player.winui.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using ReactiveUI;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace majestic_player.winui.Views
{
    public sealed partial class LibraryTabView : Page
    {
        public LibraryTabViewModel DataContext { get; set; } = new();

        public LibraryTabView()
        {
            this.InitializeComponent();

            WeakReferenceMessenger.Default.Register<Msg_ListView_SelectItems>(this, (r, msg) =>
            {
                SetSelectedIndex(msg.Index);
            });
        }

        public void SetSelectedIndex(int i)
        {
            try
            {
                TracksList.SelectedIndex = i;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }

    }

    public class Msg_ListView_SelectItems
    {
        public int Index{ get; set; }
    }
}
