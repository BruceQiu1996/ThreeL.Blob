using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;
using ThreeL.Blob.Clients.Win.Pages;

namespace ThreeL.Blob.Clients.Win.ViewModels
{
    public class MainWindowViewModel : ObservableObject
    {
        public RelayCommand ShiftSettingsPageCommand { get; set; }
        public RelayCommand ShiftMainPageCommand { get; set; }
        public RelayCommand ShiftTransferPageCommand { get; set; }
        public AsyncRelayCommand LoadCommandAsync { get; set; }

        private readonly MainPage _mainPage;
        private readonly TransferPage _transferPage;
        private readonly SettingsPage _settingsPage;
        public MainWindowViewModel(MainPage mainPage, TransferPage transferPage, SettingsPage settingsPage)
        {
            _mainPage = mainPage;
            _transferPage = transferPage;
            _settingsPage = settingsPage;
            ShiftSettingsPageCommand = new RelayCommand(OpenSettingsPage);
            ShiftMainPageCommand = new RelayCommand(OpenMainPage);
            ShiftTransferPageCommand = new RelayCommand(OpenTransferPage);
            LoadCommandAsync = new AsyncRelayCommand(LoadAsync);
        }

        private System.Windows.Controls.Page _currentPage;
        public System.Windows.Controls.Page CurrentPage
        {
            get => _currentPage;
            set => SetProperty(ref _currentPage, value);
        }

        private Task LoadAsync()
        {
            CurrentPage = _mainPage;

            return Task.CompletedTask;
        }

        private void OpenSettingsPage()
        {
            CurrentPage = _settingsPage;
        }

        private void OpenMainPage()
        {
            CurrentPage = _mainPage;
        }

        private void OpenTransferPage()
        {
            CurrentPage = _transferPage;
        }
    }
}
