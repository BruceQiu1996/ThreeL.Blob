using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using ThreeL.Blob.Clients.Grpc.Protos;
using ThreeL.Blob.Clients.Win.Dtos;
using ThreeL.Blob.Clients.Win.Entities;
using ThreeL.Blob.Clients.Win.Helpers;
using ThreeL.Blob.Clients.Win.Request;
using ThreeL.Blob.Clients.Win.Resources;
using ThreeL.Blob.Infra.Core.Extensions.System;
using ThreeL.Blob.Infra.Core.Serializers;

namespace ThreeL.Blob.Clients.Win.ViewModels.Item
{
    public class UploadItemViewModel : ObservableObject
    {
        private CancellationTokenSource _tokenSource; //取消
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

        public BitmapImage Icon => App.ServiceProvider!.GetRequiredService<FileHelper>().GetIconByFileExtension(FileName);
        public AsyncRelayCommand ResumeCommandAsync { get; set; }
        public RelayCommand PauseCommand { get; set; }
        public AsyncRelayCommand CancelCommandAsync { get; set; }

        public ProgressBar ProgressBar;

        private readonly GrpcService _grpcService;
        private readonly IDbContextFactory<MyDbContext> _dbContextFactory;
        private readonly HttpRequest _httpRequest;
        public UploadItemViewModel(GrpcService grpcService, IDbContextFactory<MyDbContext> dbContextFactory, HttpRequest httpRequest)
        {
            _grpcService = grpcService;
            _httpRequest = httpRequest;
            _dbContextFactory = dbContextFactory;
            ResumeCommandAsync = new AsyncRelayCommand(ResumeAsync);
            PauseCommand = new RelayCommand(Pause);
            CancelCommandAsync = new AsyncRelayCommand(CancelAsync);
        }

        public async Task StartAsync()
        {
            _tokenSource = new CancellationTokenSource();
            _uploadTask = Task.Run(async () =>
            {
                var resp = await _grpcService
                    .UploadFileAsync(_tokenSource.Token, FileLocation, FileId, TransferBytes, 1024 * 1024, OccurException, SetTransferBytes);
                if (resp.Result)
                {

                }
                else if (resp.Status == UploadFileResponseStatus.PauseStatus)
                {
                    CanSuspend = false;
                    using (var context = _dbContextFactory.CreateDbContext())
                    {
                        var record = await context.UploadFileRecords.FirstOrDefaultAsync(x => x.Id == Id);
                        if (record != null)
                        {
                            record.Status = Status.Suspend;
                            record.TransferBytes = TransferBytes;

                            await context.SaveChangesAsync();
                        }
                    }
                }
            });

            CanSuspend = true;
            await _uploadTask;
        }

        private async Task ResumeAsync()
        {
            if (!File.Exists(FileLocation))
                return;
            //请求远端,本地是否启动任务
            var resp = await _httpRequest.GetAsync(string.Format(Const.UPLOADING_STATUS, FileId));
            if (resp != null && resp.IsSuccessStatusCode)
            {
                var status = JsonSerializer
                    .Deserialize<FileUploadingStatusDto>(await resp.Content.ReadAsStringAsync(), SystemTextJsonSerializer.GetDefaultOptions());

                using (var fs = new FileStream(FileLocation, FileMode.Open, FileAccess.Read))
                {
                    if (status.Code != fs.ToSHA256())
                    {
                        return;
                    }
                }
                if (status != null)
                {
                    TransferBytes = status.UploadedBytes;
                    await StartAsync();
                }
            }
        }

        /// <summary>
        /// 暂停
        /// </summary>
        /// <returns></returns>
        private void Pause()
        {
            //var resp = await _httpRequest.PutAsync(string.Format(Const.UPLOADING_PAUSE, FileId), null);
            _tokenSource?.Cancel();
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

        public void OccurException(string exception)
        {
            Pause();
        }
    }
}
