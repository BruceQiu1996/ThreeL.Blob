using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;
using System.Windows.Forms;
using ThreeL.Blob.Clients.Win.Resources;

namespace ThreeL.Blob.Clients.Win.ViewModels.Page
{
    public class SettingsPageViewModel : ObservableObject
    {
        public AsyncRelayCommand LoadCommandAsync { get; set; }
        public AsyncRelayCommand ChooseDownloadFolderCommandAsync { get; set; }
        public AsyncRelayCommand ChooseTempFolderCommandAsync { get; set; }
        public AsyncRelayCommand ModifyMaxUploadThreadsCommandAsync { get; set; }
        public AsyncRelayCommand ModifyMaxDownloadThreadsCommandAsync { get; set; }

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

        private readonly IniSettings _iniSettings;
        public SettingsPageViewModel(IniSettings iniSettings)
        {
            _iniSettings = iniSettings;
            LoadCommandAsync = new AsyncRelayCommand(LoadAsync);
            ChooseDownloadFolderCommandAsync = new AsyncRelayCommand(ChooseDownloadFolderAsync);
            ChooseTempFolderCommandAsync = new AsyncRelayCommand(ChooseTempFolderAsync);
            ModifyMaxUploadThreadsCommandAsync = new AsyncRelayCommand(ModifyMaxUploadThreadsAsync);
            ModifyMaxDownloadThreadsCommandAsync = new AsyncRelayCommand(ModifyMaxDownloadThreadsAsync);
        }

        private Task LoadAsync()
        {
            DownloadLocation = _iniSettings.DownloadLocation;
            MaxUploadThreads = _iniSettings.MaxUploadThreads;
            MaxDownloadThreads = _iniSettings.MaxDownloadThreads;
            TempLocation = _iniSettings.TempLocation;

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
    }
}
