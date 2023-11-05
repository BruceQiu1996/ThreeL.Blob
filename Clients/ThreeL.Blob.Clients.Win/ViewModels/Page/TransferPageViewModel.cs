using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using ThreeL.Blob.Clients.Win.Pages;
using ThreeL.Blob.Clients.Win.Resources;
using ThreeL.Blob.Clients.Win.ViewModels.Item;

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

        private int _uploadingTasksCount;
        public int UploadingTasksCount
        {
            get => _uploadingTasksCount;
            set => SetProperty(ref _uploadingTasksCount, value);
        }

        private int _downloadingTasksCount;
        public int DownloadingTasksCount
        {
            get => _downloadingTasksCount;
            set => SetProperty(ref _downloadingTasksCount, value);
        }

        private string _uploadingTasksCountText;
        public string UploadingTasksCountText
        {
            get => _uploadingTasksCountText;
            set => SetProperty(ref _uploadingTasksCountText, value);
        }

        private string _downloadingTasksCountText;
        public string DownloadingTasksCountText
        {
            get => _downloadingTasksCountText;
            set => SetProperty(ref _downloadingTasksCountText, value);
        }

        public TransferPageViewModel(UploadingPage uploadingPage, DownloadingPage downloadingPage, TransferComplete transferComplete)
        {
            _uploadingPage = uploadingPage;
            _downloadingPage = downloadingPage;
            _transferComplete = transferComplete;
            LoadCommandAsync = new AsyncRelayCommand(LoadAsync);
            ShiftUploadingPageCommand = new RelayCommand(OpenUploadingPage);
            ShifDownloadingPageCommand = new RelayCommand(OpenDownloadingPage);
            ShiftCompletePageCommand = new RelayCommand(OpenCompletePage);

            //通知上传文件的数量
            WeakReferenceMessenger.Default.Register<TransferPageViewModel, ObservableCollection<UploadItemViewModel>, string>(this, Const.NotifyUploadingCount, async (x, y) =>
            {
                UploadingTasksCount = y.Count;
                if (UploadingTasksCount == 0)
                {
                    UploadingTasksCountText = null;
                }
                else
                {
                    UploadingTasksCountText = UploadingTasksCount > 99 ? "99+" : $"{UploadingTasksCount}";
                }
            });

            //通知下载文件的数量
            WeakReferenceMessenger.Default.Register<TransferPageViewModel, ObservableCollection<DownloadItemViewModel>, string>(this, Const.NotifyDownloadingCount, async (x, y) =>
            {
                DownloadingTasksCount = y.Count;
                if (DownloadingTasksCount == 0)
                {
                    DownloadingTasksCountText = null;
                }
                else
                {
                    DownloadingTasksCountText = DownloadingTasksCount > 99 ? "99+" : $"{DownloadingTasksCount}";
                }
            });
        }

        private async Task LoadAsync()
        {
            CurrentPage = _uploadingPage;
            await (_uploadingPage.DataContext as UploadingPageViewModel)!.LoadCommandAsync.ExecuteAsync(null);
            await (_downloadingPage.DataContext as DownloadingPageViewModel)!.LoadCommandAsync.ExecuteAsync(null);
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
