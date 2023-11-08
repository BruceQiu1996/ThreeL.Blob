using AutoMapper;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Media.Imaging;
using ThreeL.Blob.Clients.Win.Entities;
using ThreeL.Blob.Clients.Win.Helpers;
using ThreeL.Blob.Clients.Win.Profiles;
using ThreeL.Blob.Clients.Win.Request;
using ThreeL.Blob.Clients.Win.Resources;
using ThreeL.Blob.Clients.Win.ViewModels.Page;
using ThreeL.Blob.Infra.Core.Extensions.System;
using ThreeL.Blob.Infra.Core.Utils;
using ThreeL.Blob.Shared.Domain.Metadata.FileObject;

namespace ThreeL.Blob.Clients.Win.ViewModels.Item
{
    public class DownloadItemViewModel : ObservableObject
    {
        private CancellationTokenSource _pauseTokenSource; //暂停
        private CancellationTokenSource _cancelTokenSource; //取消
        private Task _downloadTask;
        public string Id { get; set; }
        public string TaskId { get; set; }
        public string TempFileLocation { get; set; }
        public string Location { get; set; }
        public string FileName { get; set; }
        public long Size { get; set; }

        private long _transferBytes;
        public long TransferBytes
        {
            get => _transferBytes;
            set
            {
                _transferBytes = value;
                Progress = TransferBytes / (double)Size * 100;
            }
        }
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

        private string message;
        public string Message
        {
            get => message;
            set => SetProperty(ref message, value);
        }

        public string Code { get; set; }

        public DateTime CreateTime { get; set; } = DateTime.UtcNow;
        public DateTime DownloadFinishTime { get; set; }
        private FileDownloadingStatus _status;
        public FileDownloadingStatus Status 
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        public BitmapImage Icon => App.ServiceProvider!.GetRequiredService<FileHelper>().GetIconByFileExtension(FileName).Item2;
        public bool OpenWhenComplete { get; set; }
        public AsyncRelayCommand ResumeCommandAsync { get; set; }
        public AsyncRelayCommand PauseCommand { get; set; }
        public AsyncRelayCommand CancelCommandAsync { get; set; }

        private readonly GrpcService _grpcService;
        private readonly HttpRequest _httpRequest;
        private readonly DatabaseHelper _databaseHelper;
        private readonly IMapper _mapper;
        private readonly IniSettings _iniSettings;
        public DownloadItemViewModel(GrpcService grpcService, 
                                     HttpRequest httpRequest,
                                     IMapper mapper,
                                     DatabaseHelper databaseHelper,
                                     IniSettings iniSettings)
        {
            _mapper = mapper;
            _grpcService = grpcService;
            _httpRequest = httpRequest;
            PauseCommand = new AsyncRelayCommand(PauseAsync);
            CancelCommandAsync = new AsyncRelayCommand(CancelAsync);
            ResumeCommandAsync = new AsyncRelayCommand(ResumeAsync);
            _databaseHelper = databaseHelper;
            _iniSettings = iniSettings;
        }

        public async Task StartAsync()
        {
            lock (Const.UploadRunTaskLock)
            {
                if (_iniSettings.MaxDownloadThreads <= App.ServiceProvider!
                    .GetRequiredService<DownloadingPageViewModel>().DownloadItemViewModels
                    .Count(x => x.Status == FileDownloadingStatus.Downloading))
                {
                    return;
                }

                _pauseTokenSource = new CancellationTokenSource();
                _cancelTokenSource = new CancellationTokenSource();
                _downloadTask = Task.Run(async () =>
                {
                    //开始下载
                    await UpdateStatusAsync(FileDownloadingStatus.Downloading);
                    Message = null;
                    await _grpcService
                        .DownloadFileAsync(_pauseTokenSource.Token, _cancelTokenSource.Token,
                        TempFileLocation, TaskId, OnComplete, OnPause, OnError, OccurException, SetTransferBytes);
                });

                CanSuspend = true;
            }

            await _downloadTask;
        }

        private async Task ResumeAsync()
        {
            if (Status != FileDownloadingStatus.DownloadingSuspend)
                return;

            await UpdateStatusAsync(FileDownloadingStatus.Wait);
        }

        private async Task PauseAsync()
        {
            await UpdateStatusAsync(FileDownloadingStatus.DownloadingSuspend);
            _pauseTokenSource?.Cancel();
        }

        private async void OnPause() 
        {
            Message = "下载暂停";
            CanSuspend = false;
        }

        private async Task CancelAsync()
        {
            if (Status == FileDownloadingStatus.DownloadingComplete || Status == FileDownloadingStatus.DownloadingFaild)
                return;

            await _httpRequest.PutAsync(string.Format(Const.CANCEL_DOWNLOADING, TaskId), null);
            await UpdateStatusAsync(FileDownloadingStatus.DownloadingComplete);
            _cancelTokenSource?.Cancel();
            Message = "取消下载";
            if (await ExpressionWaiter.WaitInTime(() => _downloadTask == null || _downloadTask.IsCompleted))
            {
                if (File.Exists(TempFileLocation))
                {
                    File.Delete(TempFileLocation);
                }
            }
        }

        internal async Task UpdateStatusAsync(FileDownloadingStatus fileStatus, string fileLocation = null)
        {
            Status = fileStatus;
            WeakReferenceMessenger.Default.Send(string.Empty, Const.StartNewDownloadTask);
            var record = await _databaseHelper.QueryFirstOrDefaultAsync<DownloadFileRecord>("SELECT * FROM DownloadFileRecords WHERE ID = @Id", new
            {
                Id
            });
            if (record != null)
            {
                List<(string, object)> sqls = new List<(string, object)>();
                record.Status = fileStatus;
                if (fileStatus == FileDownloadingStatus.DownloadingComplete || fileStatus == FileDownloadingStatus.DownloadingFaild)
                {
                    record.DownloadFinishTime = DateTime.UtcNow;
                    DownloadFinishTime = record.DownloadFinishTime;

                    var transferRecord = _mapper.Map<TransferCompleteRecord>(record);
                    transferRecord.TaskId = record.Id;
                    transferRecord.Description = TransferProfile.GetDescriptionByDownloadStatus(fileStatus);
                    transferRecord.Success = fileStatus == FileDownloadingStatus.DownloadingComplete;
                    transferRecord.Reason = Message;
                    transferRecord.FileLocation = fileLocation;

                    //context.TransferCompleteRecords.Add(transferRecord);
                    sqls.Add(("INSERT INTO TransferCompleteRecords(Id,TaskId,FileId,FileName,FileLocation,BeginTime,FinishTime,Description,IsUpload,Success,Reason)" +
                        " VALUES(@Id,@TaskId,@FileId,@FileName,@FileLocation,@BeginTime,@FinishTime,@Description,@IsUpload,@Success,@Reason)", transferRecord));

                    WeakReferenceMessenger.Default.Send(new Tuple<DownloadItemViewModel, TransferCompleteRecord>(this, transferRecord), Const.DownloadFinish);
                }

                if (fileStatus == FileDownloadingStatus.DownloadingSuspend)
                {
                    record.TransferBytes = TransferBytes;
                }

                sqls.Add(("UPDATE DownloadFileRecords SET DownloadFinishTime = @DownloadFinishTime,Status=@Status,TransferBytes=@TransferBytes WHERE ID = @Id", record));
                
                _databaseHelper.ExcuteMulti(sqls);
            }
        }

        public void SetTransferBytes(long bytes)
        {
            TransferBytes = bytes;
            Progress = TransferBytes / (double)Size * 100;
        }

        public async void OnError(string error)
        {
            Message = error;
            await UpdateStatusAsync(FileDownloadingStatus.DownloadingFaild);
        }

        public async void OccurException(string exception)
        {
            Message = "下载出现异常";
            await UpdateStatusAsync(FileDownloadingStatus.DownloadingSuspend);
        }

        private async void OnComplete()
        {
            //校验文件是否缺失
            using (var fs = File.OpenRead(TempFileLocation))
            {
                if (Code != fs.ToSHA256())
                {
                    Message = "文件损坏";
                    await UpdateStatusAsync(FileDownloadingStatus.DownloadingFaild);

                    return;
                }
            }

            //文件改名
            var fileLocation = FileName.GetAvailableFileLocation(new FileInfo(TempFileLocation).DirectoryName);
            File.Move(TempFileLocation, fileLocation);

            await UpdateStatusAsync(FileDownloadingStatus.DownloadingComplete, fileLocation);
            if (OpenWhenComplete)
            {
                Process process = new Process();
                ProcessStartInfo processStartInfo = new ProcessStartInfo(fileLocation);
                processStartInfo.UseShellExecute = true;
                process.StartInfo = processStartInfo;
                process.Start();
            }
        }
    }
}
