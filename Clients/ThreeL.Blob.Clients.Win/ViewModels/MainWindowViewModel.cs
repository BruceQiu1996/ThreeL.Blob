using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Options;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using ThreeL.Blob.Clients.Win.Configurations;
using ThreeL.Blob.Clients.Win.Helpers;
using ThreeL.Blob.Clients.Win.Pages;
using ThreeL.Blob.Clients.Win.Request;
using ThreeL.Blob.Clients.Win.Resources;
using ThreeL.Blob.Clients.Win.ViewModels.Item;
using ThreeL.Blob.Clients.Win.ViewModels.Page;
using ThreeL.Blob.Clients.Win.Windows;
using ThreeL.Blob.Shared.Domain.Metadata.User;

namespace ThreeL.Blob.Clients.Win.ViewModels
{
    public class MainWindowViewModel : ObservableObject
    {
        public RelayCommand ShiftSettingsPageCommand { get; set; }
        public RelayCommand ShiftMainPageCommand { get; set; }
        public RelayCommand ShiftTransferPageCommand { get; set; }
        public RelayCommand ShiftSharePageCommand { get; set; }
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

        private bool _isAdmin;
        public bool IsAdmin
        {
            get => _isAdmin;
            set => SetProperty(ref _isAdmin, value);
        }

        private string _userName;
        public string UserName
        {
            get => _userName;
            set => SetProperty(ref _userName, value);
        }

        private BitmapImage _avatar;
        public BitmapImage Avatar
        {
            get => _avatar;
            set => SetProperty(ref _avatar, value);
        }

        private bool _isUploadingReadyExit;
        private bool _isDownloadingReadyExit = true;
        private readonly MainPage _mainPage;
        private readonly TransferPage _transferPage;
        private readonly SettingsPage _settingsPage;
        private readonly Share _sharePage;
        private readonly ApiHttpRequest _httpRequest;
        private readonly GrowlHelper _growlHelper;
        private readonly FileHelper _fileHelper;
        private readonly Chat _chat;
        public MainWindowViewModel(MainPage mainPage, 
                                   TransferPage transferPage, 
                                   SettingsPage settingsPage,
                                   ApiHttpRequest httpRequest,
                                   GrowlHelper growlHelper,
                                   FileHelper fileHelper,
                                   Chat chat,
                                   Share sharePage)
        {
            IsAdmin = App.UserProfile.Role == Role.Admin.ToString() || App.UserProfile.Role == Role.SuperAdmin.ToString();
            _mainPage = mainPage;
            _transferPage = transferPage;
            _settingsPage = settingsPage;
            _httpRequest = httpRequest;
            _growlHelper = growlHelper;
            _fileHelper = fileHelper;
            _chat = chat;
            _sharePage = sharePage;
            ShiftSettingsPageCommand = new RelayCommand(OpenSettingsPage);
            ShiftMainPageCommand = new RelayCommand(OpenMainPage);
            ShiftTransferPageCommand = new RelayCommand(OpenTransferPage);
            ShiftSharePageCommand = new RelayCommand(OpenSharedPage);
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

            WeakReferenceMessenger.Default.Register<MainWindowViewModel, ObservableCollection<DownloadItemViewModel>, string>(this, Const.NotifyDownloadingCount, async (x, y) =>
            {
                DownloadingTasksCount = y.Count;
                if (UploadingTasksCount + DownloadingTasksCount == 0)
                    TransferCounts = null;
                else
                    TransferCounts = UploadingTasksCount + DownloadingTasksCount > 99 ? " 99+" : $"{UploadingTasksCount + DownloadingTasksCount}";
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

            WeakReferenceMessenger.Default.Register<MainWindowViewModel, string, string>(this, Const.ExitToLogin, async (x, y) =>
            {
                Process p = new Process();
                p.StartInfo.FileName = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "HeadDisk.exe");
                p.StartInfo.UseShellExecute = false;
                p.Start();

                WeakReferenceMessenger.Default.Send(string.Empty, Const.Exit);
            });

            //上传头像成功
            WeakReferenceMessenger.Default.Register<MainWindowViewModel, byte[], string>(this, Const.AvatarUploaded, async (x, y) =>
            {
                Avatar =  _fileHelper.BytesToImage(y);
            });
        }

        private System.Windows.Controls.Page _currentPage;
        public System.Windows.Controls.Page CurrentPage
        {
            get => _currentPage;
            set => SetProperty(ref _currentPage, value);
        }

        private async Task LoadAsync()
        {
            UserName = App.UserProfile.UserName;
            if (App.UserProfile.Avatar != null) 
            {
                var avatarResp = await _httpRequest.GetAsync(string.Format(Const.GET_AVATAR_IMAGE, App.UserProfile.Avatar.Replace("\\","/")));
                if (avatarResp != null) 
                {
                    Avatar =  _fileHelper.BytesToImage(await avatarResp.Content.ReadAsByteArrayAsync());
                }
            }
            CurrentPage = _mainPage;
            await (_transferPage.DataContext as TransferPageViewModel)!.LoadCommandAsync.ExecuteAsync(null);
            _chat.Left = SystemParameters.WorkArea.Width - _chat.Width;
            _chat.Top = 50;
            _chat.Show();
        }

        private Task ExitAsync()
        {
            WeakReferenceMessenger.Default.Send(string.Empty, Const.Exit);

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

        private void OpenSharedPage() 
        {
            CurrentPage = _sharePage;
        }
    }
}
