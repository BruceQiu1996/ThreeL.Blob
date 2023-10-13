using System.Windows.Controls;
using ThreeL.Blob.Clients.Win.ViewModels.Page;

namespace ThreeL.Blob.Clients.Win.Pages
{
    /// <summary>
    /// Interaction logic for DownloadingPage.xaml
    /// </summary>
    public partial class DownloadingPage : Page
    {
        public DownloadingPage(DownloadingPageViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
