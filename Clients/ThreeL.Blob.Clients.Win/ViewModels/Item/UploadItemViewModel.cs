using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using ThreeL.Blob.Clients.Win.Entities;
using ThreeL.Blob.Clients.Win.Helpers;
using ThreeL.Blob.Clients.Win.Request;
using ThreeL.Blob.Infra.Core.Extensions.System;

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
        public long Size { get; set; }
        public string SizeText => Size.ToSizeText();
        private double _progress;
        public double Progress
        {
            get => _progress;
            set => SetProperty(ref _progress, value);
        }
        private bool _canSuspend;
        public bool CanSuspend
        {
            get => _canSuspend;
            set => SetProperty(ref _canSuspend, value);
        }

        public BitmapImage Icon => GetIconByFileExtension(App.ServiceProvider!.GetRequiredService<FileHelper>().GetIconByFileExtension(FileName));
        public AsyncRelayCommand ResumeCommandAsync { get; set; }
        public AsyncRelayCommand PauseCommandAsync { get; set; }
        public AsyncRelayCommand CancelCommandAsync { get; set; }

        public ProgressBar ProgressBar;

        private readonly GrpcService _grpcService;
        private readonly IDbContextFactory<MyDbContext> _dbContextFactory;
        public UploadItemViewModel(GrpcService grpcService,IDbContextFactory<MyDbContext> dbContextFactory)
        {
            _grpcService = grpcService;
            _dbContextFactory = dbContextFactory;
            _tokenSource = new CancellationTokenSource();
            _resetEvent = new ManualResetEvent(true);
            ResumeCommandAsync = new AsyncRelayCommand(ResumeAsync);
            PauseCommandAsync = new AsyncRelayCommand(PauseAsync);
            CancelCommandAsync = new AsyncRelayCommand(CancelAsync);
        }

        public async Task StartAsync()
        {
            _uploadTask = Task.Run(async () =>
            {
                await _grpcService
                    .UploadFileAsync(_tokenSource.Token, _resetEvent, FileLocation, FileId, TransferBytes, 1024 * 1024, SetTransferBytes);
            });

            CanSuspend = true;
            await _uploadTask;
        }

        private async Task ResumeAsync()
        {
            //请求远端,本地是否启动任务

            _resetEvent.Set();
            CanSuspend = true;
        }

        /// <summary>
        /// 暂停
        /// </summary>
        /// <returns></returns>
        private async Task PauseAsync()
        {
            _resetEvent.Reset();
            CanSuspend = false;
            using (var context = _dbContextFactory.CreateDbContext()) 
            {
                var record = await context.UploadFileRecords.FirstOrDefaultAsync(x=>x.Id== Id);
                if (record != null)
                {
                    record.Status = Status.Suspend;
                    record.TransferBytes = TransferBytes;

                    await context.SaveChangesAsync();
                }
            }
        }

        private async Task CancelAsync()
        {
            _tokenSource.Cancel();
            //TODO请求远端
            //TODO本地处理
            _uploadTask.Dispose();
        }

        public void SetTransferBytes(long bytes)
        {
            TransferBytes = bytes;
            Progress = TransferBytes / (double)Size * 100;
        }

        private BitmapImage GetIconByFileExtension(string imageName) 
        {
            var source = new BitmapImage();
            try
            {
                string imgUrl = $"pack://application:,,,/ThreeL.Blob.Clients.Win;component/Images/{imageName}";
                source.BeginInit();
                source.UriSource = new Uri(imgUrl, UriKind.RelativeOrAbsolute);
                source.EndInit();

                return source;
            }
            finally
            {
                source.Freeze();
            }
        }
    }
}
