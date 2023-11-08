using AutoMapper;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using ThreeL.Blob.Clients.Grpc.Protos;
using ThreeL.Blob.Clients.Win.Dtos;
using ThreeL.Blob.Clients.Win.Entities;
using ThreeL.Blob.Clients.Win.Helpers;
using ThreeL.Blob.Clients.Win.Profiles;
using ThreeL.Blob.Clients.Win.Request;
using ThreeL.Blob.Clients.Win.Resources;
using ThreeL.Blob.Clients.Win.ViewModels.Page;
using ThreeL.Blob.Infra.Core.Extensions.System;
using ThreeL.Blob.Infra.Core.Serializers;
using ThreeL.Blob.Shared.Domain.Metadata.FileObject;

namespace ThreeL.Blob.Clients.Win.ViewModels.Item
{
    public class UploadItemViewModel : ObservableObject
    {
        private CancellationTokenSource _pauseTokenSource; //取消
        private CancellationTokenSource _cancelTokenSource; //取消
        private Task _uploadTask;
        public string Id { get; set; }
        public long FileId { get; set; }
        public string FileLocation { get; set; }
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

        public DateTime CreateTime { get; set; } = DateTime.UtcNow;
        public DateTime UploadFinishTime { get; set; }

        private FileUploadingStatus _status;
        public FileUploadingStatus Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        public BitmapImage Icon => App.ServiceProvider!.GetRequiredService<FileHelper>().GetIconByFileExtension(FileName).Item2;
        public AsyncRelayCommand ResumeCommandAsync { get; set; }
        public AsyncRelayCommand PauseCommand { get; set; }
        public AsyncRelayCommand CancelCommandAsync { get; set; }

        private readonly GrpcService _grpcService;
        private readonly HttpRequest _httpRequest;
        private readonly IMapper _mapper;
        private readonly IniSettings _iniSettings;
        private readonly DatabaseHelper _databaseHelper;
        public UploadItemViewModel(GrpcService grpcService, 
                                   HttpRequest httpRequest,
                                   IMapper mapper, IniSettings iniSettings, DatabaseHelper databaseHelper)
        {
            _mapper = mapper;
            _grpcService = grpcService;
            _httpRequest = httpRequest;
            _iniSettings = iniSettings;
            ResumeCommandAsync = new AsyncRelayCommand(ResumeAsync);
            PauseCommand = new AsyncRelayCommand(Pause);
            CancelCommandAsync = new AsyncRelayCommand(CancelAsync);
            _databaseHelper = databaseHelper;
        }

        public async Task StartAsync()
        {
            lock (Const.UploadRunTaskLock)
            {
                if (_iniSettings.MaxUploadThreads <= App.ServiceProvider!
                    .GetRequiredService<UploadingPageViewModel>().UploadItemViewModels.Count(x => x.Status == FileUploadingStatus.Uploading))
                {
                    return;
                }

                _pauseTokenSource = new CancellationTokenSource();
                _cancelTokenSource = new CancellationTokenSource();
                _uploadTask = Task.Run(async () =>
                {
                    //开始上传
                    await UpdateStatusAsync(FileUploadingStatus.Uploading);
                    Message = null;
                    var resp = await _grpcService
                        .UploadFileAsync(_pauseTokenSource.Token, _cancelTokenSource.Token, FileLocation, FileId, TransferBytes, 1024 * 1024, OccurException, SetTransferBytes);
                    if (resp.Result)
                    {
                        await UpdateStatusAsync(FileUploadingStatus.UploadingComplete);
                        Message = "上传成功";
                    }
                    else if (resp.Status == UploadFileResponseStatus.PauseStatus) //暂停(主动暂停，网络问题等)
                    {
                        CanSuspend = false;
                        await UpdateStatusAsync(FileUploadingStatus.UploadingSuspend);
                        Message = resp.Message;
                    }
                    else if (resp.Status == UploadFileResponseStatus.ErrorStatus) //不可知的异常
                    {
                        await UpdateStatusAsync(FileUploadingStatus.UploadingFaild);
                        Message = resp.Message;
                    }
                    //else if (resp.Status == UploadFileResponseStatus.CancelStatus) //不可知的异常
                    //{
                    //    await UpdateStatusAsync(FileUploadingStatus.UploadingFaild);
                    //    Message = resp.Message;
                    //}
                });

                CanSuspend = true;
            }

            await _uploadTask;
        }

        /// <summary>
        /// 恢复上传
        /// </summary>
        /// <returns></returns>
        private async Task ResumeAsync()
        {
            if (Status != FileUploadingStatus.UploadingSuspend)
                return;

            if (!File.Exists(FileLocation))
            {
                await CancelUploadingAsync("本地文件不存在");

                return;
            }
            //请求远端,本地是否启动任务
            var resp = await _httpRequest.GetAsync(string.Format(Const.UPLOADING_STATUS, FileId));
            if (resp != null && resp.IsSuccessStatusCode)
            {
                var status = JsonSerializer
                    .Deserialize<FileUploadingStatusDto>(await resp.Content.ReadAsStringAsync(), SystemTextJsonSerializer.GetDefaultOptions());

                using (var fs = new FileStream(FileLocation, FileMode.Open, FileAccess.Read))
                {
                    if (status!.Code != fs.ToSHA256())
                    {
                        await CancelUploadingAsync("文件已被修改");
                        return;
                    }
                }
                if (status != null)
                {
                    TransferBytes = status.UploadedBytes;
                    await UpdateStatusAsync(FileUploadingStatus.Wait);
                }
            }
        }

        /// <summary>
        /// 暂停
        /// </summary>
        /// <returns></returns>
        private async Task Pause()
        {
            await UpdateStatusAsync(FileUploadingStatus.UploadingSuspend);
            _pauseTokenSource?.Cancel();
        }

        private async Task CancelAsync()
        {
            await CancelUploadingAsync();
        }

        private async Task CancelUploadingAsync(string reason = "文件上传取消") 
        {
            if (Status == FileUploadingStatus.UploadingComplete || Status == FileUploadingStatus.UploadingFaild)
                return;

            _cancelTokenSource?.Cancel();
            var resp = await _httpRequest.PutAsync(string.Format(Const.CANCEL_UPLOADING, FileId), null);
            if (resp != null && resp.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<FileUploadingStatusDto>(await resp.Content.ReadAsStringAsync(), SystemTextJsonSerializer.GetDefaultOptions());
                if (result.Status == FileStatus.Cancel)
                {
                    Message = reason;
                    await UpdateStatusAsync(FileUploadingStatus.UploadingFaild);
                }
            }
        }

        public void SetTransferBytes(long bytes)
        {
            TransferBytes = bytes;
            Progress = TransferBytes / (double)Size * 100;
        }

        public async void OccurException(string exception)
        {
            await Pause();
            await UpdateStatusAsync(FileUploadingStatus.UploadingSuspend);
            Message = "网络传输异常";
        }

        private async Task UpdateStatusAsync(FileUploadingStatus fileStatus)
        {
            Status = fileStatus;
            WeakReferenceMessenger.Default.Send(string.Empty, Const.StartNewUploadTask);
            //var record = context.UploadFileRecords.FirstOrDefault(x => x.Id == Id);
            var record = await _databaseHelper.QueryFirstOrDefaultAsync<UploadFileRecord>("SELECT * FROM UploadFileRecords WHERE ID = @Id", new
            {
                Id
            });
            if (record != null)
            {
                List<(string, object)> sqls = new List<(string, object)>();
                record.Status = fileStatus;
                if (fileStatus == FileUploadingStatus.UploadingComplete || fileStatus == FileUploadingStatus.UploadingFaild)
                {
                    record.UploadFinishTime = DateTime.UtcNow;
                    UploadFinishTime = record.UploadFinishTime;

                    var transferRecord = _mapper.Map<TransferCompleteRecord>(record);
                    transferRecord.TaskId = record.Id;
                    transferRecord.Description = TransferProfile.GetDescriptionByUploadStatus(fileStatus);
                    transferRecord.Success = fileStatus == FileUploadingStatus.UploadingComplete;
                    transferRecord.Reason = Message;

                    //context.TransferCompleteRecords.Add(transferRecord);
                    sqls.Add(("INSERT INTO TransferCompleteRecords (Id,TaskId,FileId,FileName,FileLocation,BeginTime,FinishTime,Description,IsUpload,Success,Reason)" +
                        "VALUES(@Id,@TaskId,@FileId,@FileName,@FileLocation,@BeginTime,@FinishTime,@Description,@IsUpload,@Success,@Reason)", transferRecord));

                    WeakReferenceMessenger.Default.Send(new Tuple<UploadItemViewModel, TransferCompleteRecord>(this, transferRecord), Const.UploadFinish);
                }

                if (fileStatus == FileUploadingStatus.UploadingSuspend)
                {
                    record.TransferBytes = TransferBytes;
                }

                sqls.Add(("UPDATE UploadFileRecords SET UploadFinishTime = @UploadFinishTime,Status=@Status,TransferBytes=@TransferBytes WHERE ID = @Id", record));

                _databaseHelper.ExcuteMulti(sqls);
            }
        }
    }
}
