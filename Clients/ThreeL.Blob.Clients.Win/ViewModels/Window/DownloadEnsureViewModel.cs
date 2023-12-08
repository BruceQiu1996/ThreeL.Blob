using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Threading.Tasks;
using System.Windows;
using ThreeL.Blob.Clients.Win.Helpers;
using ThreeL.Blob.Clients.Win.Resources;
using ThreeL.Blob.Clients.Win.Windows;
using ThreeL.Blob.Infra.Core.Extensions.System;

namespace ThreeL.Blob.Clients.Win.ViewModels.Window
{
    public class DownloadEnsureViewModel : ObservableObject
    {
        private long _allSize;
        public long AllSize
        {
            get => _allSize;
            set => SetProperty(ref _allSize, value);
        }

        private long _freeSize;
        public long FreeSize
        {
            get => _freeSize;
            set => SetProperty(ref _freeSize, value);
        }

        private string? _freeDesc;
        public string? FreeDesc
        {
            get => _freeDesc;
            set => SetProperty(ref _freeDesc, value);
        }

        private double _progress;
        public double Progress
        {
            get => _progress;
            set => SetProperty(ref _progress, value);
        }

        private string? _downloadLocation;
        public string? DownloadLocation
        {
            get => _downloadLocation;
            set
            {
                SetProperty(ref _downloadLocation, value);
            }
        }

        public AsyncRelayCommand LoadCommandAsync { get; set; }
        public RelayCommand ConfirmDownloadCommand { get; set; }
        public RelayCommand CancelDownloadCommand { get; set; }
        public DownloadEnsure DownloadEnsure { get; set; }

        private readonly FileHelper _fileHelper;
        private readonly IniSettings _iniSettings;
        public DownloadEnsureViewModel(FileHelper fileHelper, IniSettings iniSettings)
        {
            _fileHelper = fileHelper;
            _iniSettings = iniSettings;
            LoadCommandAsync = new AsyncRelayCommand(LoadAsync);
            ConfirmDownloadCommand = new RelayCommand(ConfirmDownload);
            CancelDownloadCommand = new RelayCommand(CancelDownload);
        }

        private Task LoadAsync()
        {
            DownloadLocation = _iniSettings.DownloadLocation;
            var appDir = _iniSettings.DownloadLocation;
            var disk = appDir!.Substring(0, appDir.IndexOf(':'));
            AllSize = _fileHelper.GetHardDiskSpace(disk);
            FreeSize = _fileHelper.GetHardDiskFreeSpace(disk);
            FreeDesc = $"{FreeSize.ToSizeText()}/{AllSize.ToSizeText()}";
            Progress = FreeSize * 100.0 / AllSize;

            return Task.CompletedTask;
        }

        public void ConfirmDownload()
        {
            DownloadEnsure.DialogResult = true;
        }

        public void CancelDownload()
        {
            DownloadEnsure.DialogResult = false;
        }
    }
}
