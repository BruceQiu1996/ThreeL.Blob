using Amazon.Runtime;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using ThreeL.Blob.Clients.Win.Configurations;
using ThreeL.Blob.Clients.Win.Helpers;
using ThreeL.Blob.Clients.Win.Pages;
using ThreeL.Blob.Clients.Win.Request;
using ThreeL.Blob.Clients.Win.Resources;
using ThreeL.Blob.Clients.Win.ViewModels.Item;
using ThreeL.Blob.Clients.Win.ViewModels.Page;
using ThreeL.Blob.Shared.Domain.Metadata.User;

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

        private bool _isAdmin;
        public bool IsAdmin
        {
            get => _isAdmin;
            set => SetProperty(ref _isAdmin, value);
        }

        private bool _isUploadingReadyExit;
        private bool _isDownloadingReadyExit = true;
        private readonly MainPage _mainPage;
        private readonly TransferPage _transferPage;
        private readonly SettingsPage _settingsPage;
        private readonly HttpRequest _httpRequest;
        private readonly RemoteOptions _remoteOptions;
        private readonly GrowlHelper _growlHelper;
        public MainWindowViewModel(MainPage mainPage, TransferPage transferPage, SettingsPage settingsPage, HttpRequest httpRequest,
                                   IOptions<RemoteOptions> remoteOptions,
                                   GrowlHelper growlHelper)
        {
            IsAdmin = App.UserProfile.Role == Role.Admin.ToString() || App.UserProfile.Role == Role.SuperAdmin.ToString();
            _mainPage = mainPage;
            _transferPage = transferPage;
            _settingsPage = settingsPage;
            _httpRequest = httpRequest;
            _growlHelper = growlHelper;
            _remoteOptions = remoteOptions.Value;
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
        }

        private System.Windows.Controls.Page _currentPage;
        public System.Windows.Controls.Page CurrentPage
        {
            get => _currentPage;
            set => SetProperty(ref _currentPage, value);
        }

        private async Task LoadAsync()
        {
            CurrentPage = _mainPage;
            await (_transferPage.DataContext as TransferPageViewModel)!.LoadCommandAsync.ExecuteAsync(null);
            App.HubConnection = new HubConnectionBuilder().WithUrl($"http://{_remoteOptions.Host}:{_remoteOptions.ChatPort}/Chat", option =>
            {
                option.CloseTimeout = TimeSpan.FromSeconds(60);
                option.AccessTokenProvider = () => Task.FromResult(_httpRequest._token)!;
            }).WithAutomaticReconnect().Build();

            App.HubConnection.On("LoginSuccess", () =>
            {
                _growlHelper.Success("登录聊天系统成功");
            });

            await App.HubConnection.StartAsync();
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
