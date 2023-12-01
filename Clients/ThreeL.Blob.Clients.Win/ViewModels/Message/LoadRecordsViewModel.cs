using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using ThreeL.Blob.Clients.Win.Resources;
using ThreeL.Blob.Shared.Domain.Metadata.Message;

namespace ThreeL.Blob.Clients.Win.ViewModels.Message
{
    public class LoadRecordsViewModel : MessageViewModel
    {
        public LoadRecordsViewModel() : base(MessageType.LoadRecords)
        {
            FetchHistoryChatRecordsCommand = new RelayCommand(() =>
            {
                WeakReferenceMessenger.Default.Send(string.Empty, Const.FetchHistoryChatRecords);
            });
        }

        private bool _isDisabled;
        public bool IsDisabled 
        {
            get => _isDisabled;
            set => SetProperty(ref _isDisabled, value);
        }

        public RelayCommand FetchHistoryChatRecordsCommand { get; set; }
    }
}
