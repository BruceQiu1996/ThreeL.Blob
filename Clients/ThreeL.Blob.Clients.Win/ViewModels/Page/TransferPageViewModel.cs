using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;
using ThreeL.Blob.Clients.Win.Pages;

namespace ThreeL.Blob.Clients.Win.ViewModels.Page
{
    public class TransferPageViewModel : ObservableObject
    {
        public RelayCommand ShiftUploadingPageCommand { get; set; }
        public RelayCommand ShifDownloadingPageCommand { get; set; }
        public RelayCommand ShiftCompletePageCommand { get; set; }
        public AsyncRelayCommand LoadCommandAsync { get; set; }

        private readonly UploadingPage _uploadingPage;
        private readonly DownloadingPage _downloadingPage;
        private readonly TransferComplete _transferComplete;

        public TransferPageViewModel(UploadingPage uploadingPage, DownloadingPage downloadingPage, TransferComplete transferComplete)
        {
            _uploadingPage = uploadingPage;
            _downloadingPage = downloadingPage;
            _transferComplete = transferComplete;
            ShiftUploadingPageCommand = new RelayCommand(OpenUploadingPage);
            ShifDownloadingPageCommand = new RelayCommand(OpenDownloadingPage);
            ShiftCompletePageCommand = new RelayCommand(OpenCompletePage);
            CurrentPage = _uploadingPage;
        }

        private System.Windows.Controls.Page _currentPage;
        public System.Windows.Controls.Page CurrentPage
        {
            get => _currentPage;
            set => SetProperty(ref _currentPage, value);
        }

        private void OpenUploadingPage()
        {
            CurrentPage = _uploadingPage;
        }

        private void OpenDownloadingPage()
        {
            CurrentPage = _downloadingPage;
        }

        private void OpenCompletePage()
        {
            CurrentPage = _transferComplete;
        }
    }
}
