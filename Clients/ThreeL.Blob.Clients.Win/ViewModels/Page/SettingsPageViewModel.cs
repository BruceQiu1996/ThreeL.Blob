using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using ThreeL.Blob.Clients.Win.Dtos;
using ThreeL.Blob.Clients.Win.Helpers;
using ThreeL.Blob.Clients.Win.Pages;
using ThreeL.Blob.Clients.Win.Request;
using ThreeL.Blob.Clients.Win.Resources;
using ThreeL.Blob.Infra.Core.Extensions.System;

namespace ThreeL.Blob.Clients.Win.ViewModels.Page
{
    public class SettingsPageViewModel : ObservableObject
    {
        public PasswordBox OldPasswordBox;
        public PasswordBox NewPassword;
        public PasswordBox ConfirmPasswordBox;
        public AsyncRelayCommand LoadCommandAsync { get; set; }
        public AsyncRelayCommand ChooseDownloadFolderCommandAsync { get; set; }
        public AsyncRelayCommand ChooseTempFolderCommandAsync { get; set; }
        public AsyncRelayCommand ModifyMaxUploadThreadsCommandAsync { get; set; }
        public AsyncRelayCommand ModifyMaxDownloadThreadsCommandAsync { get; set; }
        public AsyncRelayCommand ModifyPasswordAsyncCommand { get; set; }
        public AsyncRelayCommand UploadAvatarCommandAsync { get; set; }

        private string? _downloadLocation;
        public string? DownloadLocation
        {
            get => _downloadLocation;
            set
            {
                SetProperty(ref _downloadLocation, value);
            }
        }

        private string? _tempLocation;
        public string? TempLocation
        {
            get => _tempLocation;
            set
            {
                SetProperty(ref _tempLocation, value);
            }
        }

        private int _maxUploadThreads;
        public int MaxUploadThreads
        {
            get => _maxUploadThreads;
            set
            {
                SetProperty(ref _maxUploadThreads, value);
            }
        }

        private int _maxDownloadThreads;
        public int MaxDownloadThreads
        {
            get => _maxDownloadThreads;
            set
            {
                SetProperty(ref _maxDownloadThreads, value);
            }
        }

        private bool _autoStart;
        public bool AutoStart
        {
            get => _autoStart;
            set
            {
                SetProperty(ref _autoStart, value);
                _iniSettings.WriteAutoStart(value).GetAwaiter().GetResult();
            }
        }

        private bool _exitWithoutMin;
        public bool ExitWithoutMin
        {
            get => _exitWithoutMin;
            set
            {
                SetProperty(ref _exitWithoutMin, value);
                _iniSettings.WriteExitWithoutMin(value).GetAwaiter().GetResult();
            }
        }

        private bool _hiddenChatWindow;
        public bool HiddenChatWindow
        {
            get => _hiddenChatWindow;
            set
            {
                SetProperty(ref _hiddenChatWindow, value);
                _iniSettings.WriteHiddenChatWindow(value).GetAwaiter().GetResult();
                WeakReferenceMessenger.Default.Send(value ? "close" : "open", Const.HiddenChatWindow);
            }
        }
        private readonly IniSettings _iniSettings;
        private readonly GrowlHelper _growlHelper;
        private readonly ApiHttpRequest _httpRequest;
        public SettingsPageViewModel(IniSettings iniSettings, GrowlHelper growlHelper, ApiHttpRequest httpRequest)
        {
            _iniSettings = iniSettings;
            _growlHelper = growlHelper;
            _httpRequest = httpRequest;
            LoadCommandAsync = new AsyncRelayCommand(LoadAsync);
            ChooseDownloadFolderCommandAsync = new AsyncRelayCommand(ChooseDownloadFolderAsync);
            ChooseTempFolderCommandAsync = new AsyncRelayCommand(ChooseTempFolderAsync);
            ModifyMaxUploadThreadsCommandAsync = new AsyncRelayCommand(ModifyMaxUploadThreadsAsync);
            ModifyMaxDownloadThreadsCommandAsync = new AsyncRelayCommand(ModifyMaxDownloadThreadsAsync);
            ModifyPasswordAsyncCommand = new AsyncRelayCommand(ModifyPasswordAsync);
            UploadAvatarCommandAsync = new AsyncRelayCommand(UploadAvatarAsync);
        }

        private Task LoadAsync()
        {
            DownloadLocation = _iniSettings.DownloadLocation;
            MaxUploadThreads = _iniSettings.MaxUploadThreads;
            MaxDownloadThreads = _iniSettings.MaxDownloadThreads;
            TempLocation = _iniSettings.TempLocation;
            AutoStart = _iniSettings.AutoStart;
            ExitWithoutMin = _iniSettings.ExitWithoutMin;
            return Task.CompletedTask;
        }

        private async Task ChooseDownloadFolderAsync()
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            var dialog = folderBrowserDialog.ShowDialog();
            if (dialog == DialogResult.OK)
            {
                await _iniSettings.WriteDownloadLocation(folderBrowserDialog.SelectedPath);
                DownloadLocation = _iniSettings.DownloadLocation;
            }
        }

        private async Task ChooseTempFolderAsync()
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            var dialog = folderBrowserDialog.ShowDialog();
            if (dialog == DialogResult.OK)
            {
                await _iniSettings.WriteTempLocation(folderBrowserDialog.SelectedPath);
                TempLocation = _iniSettings.TempLocation;
            }
        }

        private async Task ModifyMaxUploadThreadsAsync()
        {
            await _iniSettings.WriteMaxUploadThreads(MaxUploadThreads);
        }

        private async Task ModifyMaxDownloadThreadsAsync()
        {
            await _iniSettings.WriteMaxDownloadThreads(MaxDownloadThreads);
        }

        //修改密码
        private async Task ModifyPasswordAsync()
        {
            if (string.IsNullOrEmpty(OldPasswordBox.Password) || string.IsNullOrEmpty(NewPassword.Password)
                || string.IsNullOrEmpty(ConfirmPasswordBox.Password))

            {
                _growlHelper.Warning("密码不能为空");
                return;
            }

            if (ConfirmPasswordBox.Password != NewPassword.Password)
            {
                _growlHelper.Warning("两次密码不一致");
                return;
            }

            if (!NewPassword.Password.ValidPassword())
            {
                _growlHelper.Warning("密码6-16个长度，并且需要包含数字和字母");
                return;
            }

            var resp = await _httpRequest.PutAsync(Const.MODIFY_PASSWORD, new UserModifyPasswordDto()
            {
                OldPassword = OldPasswordBox.Password,
                NewPassword = NewPassword.Password
            });

            if (resp != null)
            {
                //修改密码成功
                _growlHelper.Success("修改密码成功,3秒后退出到登录界面");
                await Task.Delay(3000);
                //退出到登录界面
                WeakReferenceMessenger.Default.Send(string.Empty, Const.ExitToLogin);
            }
        }

        //上传头像
        private async Task UploadAvatarAsync()
        {
            var avatar = App.ServiceProvider!.GetRequiredService<SettingsPage>().avatar;
            if (avatar.Uri == null || !avatar.HasValue || !File.Exists(avatar.Uri.LocalPath))
            {
                return;
            }

            var avatarInfo = new FileInfo(avatar.Uri.LocalPath);
            if (avatarInfo.Length > 2 * 1024 * 1024)
            {
                _growlHelper.Warning("图片大于2M");
                return;
            }

            var data = await File.ReadAllBytesAsync(avatar.Uri.LocalPath);
            var resp = await _httpRequest.PostAvatarAsync(avatarInfo.Name, data);
            if (resp != null)
            {
                WeakReferenceMessenger.Default.Send(await resp.Content.ReadAsByteArrayAsync(), Const.AvatarUploaded);
            }

            avatar.SetValue(HandyControl.Controls.ImageSelector.UriPropertyKey, default(Uri));
            avatar.SetValue(HandyControl.Controls.ImageSelector.PreviewBrushPropertyKey, default(Brush));
            avatar.SetValue(HandyControl.Controls.ImageSelector.HasValuePropertyKey, false);
            avatar.SetCurrentValue(FrameworkElement.ToolTipProperty, default);
            avatar.RaiseEvent(new RoutedEventArgs(HandyControl.Controls.ImageSelector.ImageUnselectedEvent, this));
        }
    }
}
