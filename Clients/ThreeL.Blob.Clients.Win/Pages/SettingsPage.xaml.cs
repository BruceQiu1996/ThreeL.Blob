using System.Windows.Controls;
using ThreeL.Blob.Clients.Win.ViewModels.Page;

namespace ThreeL.Blob.Clients.Win.Pages
{
    /// <summary>
    /// Interaction logic for SettingsPage.xaml
    /// </summary>
    public partial class SettingsPage : Page
    {
        public SettingsPage(SettingsPageViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;

            viewModel.OldPasswordBox = oldPwd;
            viewModel.NewPassword = newPwd;
            viewModel.ConfirmPasswordBox = confirmPwd;
        }
    }
}
