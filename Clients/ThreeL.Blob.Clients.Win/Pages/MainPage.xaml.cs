using System.Windows.Controls;
using ThreeL.Blob.Clients.Win.ViewModels.Page;

namespace ThreeL.Blob.Clients.Win.Pages
{
    /// <summary>
    /// Interaction logic for MainPage.xaml
    /// </summary>
    public partial class MainPage : Page
    {
        public MainPage(MainPageViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
