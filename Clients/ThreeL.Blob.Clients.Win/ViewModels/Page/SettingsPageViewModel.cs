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

        private string? _downloadLocation;
        public string? DownloadLocation
        {
            get => _downloadLocation;
            set
            {
                SetProperty(ref _downloadLocation, value);
            }
        }

        private readonly IniSettings _iniSettings;
        public SettingsPageViewModel(IniSettings iniSettings)
        {
            _iniSettings = iniSettings;
            LoadCommandAsync = new AsyncRelayCommand(LoadAsync);
            ChooseDownloadFolderCommandAsync = new AsyncRelayCommand(ChooseDownloadFolderAsync);
        }

        private async Task LoadAsync() 
        {
            DownloadLocation = _iniSettings.DownloadLocation;
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
    }
}
