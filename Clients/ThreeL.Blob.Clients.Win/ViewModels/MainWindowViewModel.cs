using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using ThreeL.Blob.Clients.Win.Pages;
using ThreeL.Blob.Clients.Win.Resources;
using ThreeL.Blob.Clients.Win.ViewModels.Item;

namespace ThreeL.Blob.Clients.Win.ViewModels
{
    public class MainWindowViewModel : ObservableObject
    {
        public RelayCommand ShiftSettingsPageCommand { get; set; }
        public RelayCommand ShiftMainPageCommand { get; set; }
        public RelayCommand ShiftTransferPageCommand { get; set; }
        public AsyncRelayCommand LoadCommandAsync { get; set; }
        public AsyncRelayCommand ExitCommandAsync { get; set; }
        private string _transferCounts;
        public string TransferCounts
        {
            get => _transferCounts;
            set => SetProperty(ref _transferCounts, value);
        }

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

        private bool _isUploadingReadyExit;
        private bool _isDownloadingReadyExit = true;
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
            ExitCommandAsync = new AsyncRelayCommand(ExitAsync);
            TransferCounts = null;
            //通知上传文件的数量
            WeakReferenceMessenger.Default.Register<MainWindowViewModel, ObservableCollection<UploadItemViewModel>, string>(this, Const.NotifyUploadingCount, async (x, y) =>
            {
                UploadingTasksCount = y.Count;
                if(UploadingTasksCount + DownloadingTasksCount == 0)
                    TransferCounts = null;
                else
                    TransferCounts = UploadingTasksCount + DownloadingTasksCount  >  99?" 99+": $"{UploadingTasksCount + DownloadingTasksCount}";
            });

            WeakReferenceMessenger.Default.Register<MainWindowViewModel, string, string>(this, Const.CanExit, async (x, y) =>
            {
                if (y == "uploading")
                    _isUploadingReadyExit = true;
                if (y == "downloading")
                    _isDownloadingReadyExit = true;

                if (_isUploadingReadyExit && _isDownloadingReadyExit)
                    await App.CloseAsync();
            });
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

        private Task ExitAsync()
        {
            WeakReferenceMessenger.Default.Send<string, string>(string.Empty, Const.Exit);

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
