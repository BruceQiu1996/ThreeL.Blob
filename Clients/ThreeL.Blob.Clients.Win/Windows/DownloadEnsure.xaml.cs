using CommunityToolkit.Mvvm.Messaging;
using System.Windows;
using System.Windows.Input;
using ThreeL.Blob.Clients.Win.Resources;
using ThreeL.Blob.Clients.Win.ViewModels.Window;

namespace ThreeL.Blob.Clients.Win.Windows
{
    /// <summary>
    /// Interaction logic for DownloadEnsure.xaml
    /// </summary>
    public partial class DownloadEnsure : Window
    {
        public DownloadEnsure(DownloadEnsureViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        private void Border_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }
    }
}
