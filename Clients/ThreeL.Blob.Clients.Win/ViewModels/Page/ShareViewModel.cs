using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Threading.Tasks;
using ThreeL.Blob.Clients.Win.Request;

namespace ThreeL.Blob.Clients.Win.ViewModels.Page
{
    public class ShareViewModel : ObservableObject
    {
        private bool _hadRecord;
        public bool HadRecord
        {
            get => _hadRecord;
            set => SetProperty(ref _hadRecord, value);
        }

        private bool _loaded = false;
        public AsyncRelayCommand LoadCommandAsync { get; set; }

        private readonly ApiHttpRequest _httpRequest;
        public ShareViewModel(ApiHttpRequest httpRequest)
        {
            _httpRequest = httpRequest;
            LoadCommandAsync = new AsyncRelayCommand(LoadAsync);
        }

        private async Task LoadAsync()
        {
            try
            {
                

                _loaded = true;
            }
            catch (Exception ex)
            {
                _loaded = false;
            }
        }
    }
}
