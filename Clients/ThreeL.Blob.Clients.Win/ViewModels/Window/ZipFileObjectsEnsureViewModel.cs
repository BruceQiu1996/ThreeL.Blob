using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ThreeL.Blob.Clients.Win.Windows;

namespace ThreeL.Blob.Clients.Win.ViewModels.Window
{
    public class ZipFileObjectsEnsureViewModel : ObservableObject
    {
        private string _zipName;
        public string ZipName
        {
            get => _zipName;
            set => SetProperty(ref _zipName, value);
        }

        public RelayCommand ConfirmDownloadCommand { get; set; }
        public RelayCommand CancelDownloadCommand { get; set; }
        public ZipFileObjectsEnsure ZipFileObjectsEnsure { get; set; }
        public ZipFileObjectsEnsureViewModel() 
        {
            ConfirmDownloadCommand = new RelayCommand(ConfirmDownload);
            CancelDownloadCommand = new RelayCommand(CancelDownload);
        }

        public void ConfirmDownload()
        {
            ZipFileObjectsEnsure.DialogResult = true;
        }

        public void CancelDownload()
        {
            ZipFileObjectsEnsure.DialogResult = false;
        }
    }
}
