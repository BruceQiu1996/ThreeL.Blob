using CommunityToolkit.Mvvm.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using ThreeL.Blob.Clients.Win.Request;

namespace ThreeL.Blob.Clients.Win.ViewModels.Item
{
    public class UploadItemViewModel : ObservableObject
    {
        private readonly CancellationTokenSource _tokenSource; //取消
        private readonly ManualResetEvent _resetEvent;//暂停
        private Task _uploadTask;
        public string Id { get; set; }
        public long FileId { get; set; }
        public string FileLocation { get; set; }
        public string FileName { get; set; }
        public long TransferBytes { get; set; }
        private long Size { get; set; }
        private double _progress;
        public double Progress
        {
            get => _progress;
            set => SetProperty(ref _progress, value);
        }

        private readonly GrpcService _grpcService;
        public UploadItemViewModel(GrpcService grpcService, ManualResetEvent resetEvent)
        {
            _grpcService = grpcService;
            _tokenSource = new CancellationTokenSource();
            _resetEvent = resetEvent;

        }

        public async Task StartAsync()
        {
            _uploadTask = Task.Run(async () =>
            {
                await _grpcService
                    .UploadFileAsync(_tokenSource.Token, _resetEvent, FileLocation, TransferBytes, 1024 * 1024, SetTransferBytes);
            });

            await _uploadTask;
        }

        public void SetTransferBytes(long bytes)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                TransferBytes = bytes;
                Progress = TransferBytes / (double)Size;
            });
        }
    }
}
