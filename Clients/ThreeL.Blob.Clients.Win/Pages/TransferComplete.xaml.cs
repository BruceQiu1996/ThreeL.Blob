using System.Windows.Controls;
using ThreeL.Blob.Clients.Win.ViewModels.Page;

namespace ThreeL.Blob.Clients.Win.Pages
{
    /// <summary>
    /// Interaction logic for TransferComplete.xaml
    /// </summary>
    public partial class TransferComplete : Page
    {
        public TransferComplete(TransferCompletePageViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
