﻿using AutoMapper;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using ThreeL.Blob.Clients.Win.Entities;
using ThreeL.Blob.Clients.Win.Helpers;
using ThreeL.Blob.Clients.Win.Profiles;
using ThreeL.Blob.Clients.Win.Request;
using ThreeL.Blob.Clients.Win.Resources;
using ThreeL.Blob.Infra.Core.Extensions.System;
using ThreeL.Blob.Infra.Core.Utils;
using ThreeL.Blob.Shared.Domain.Metadata.FileObject;

namespace ThreeL.Blob.Clients.Win.ViewModels.Item
{
    public class DownloadItemViewModel : ObservableObject
    {
        private CancellationTokenSource _pauseTokenSource; //取消
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
        public FileDownloadingStatus Status { get; set; }

        public BitmapImage Icon => App.ServiceProvider!.GetRequiredService<FileHelper>().GetIconByFileExtension(FileName).Item2;
        public AsyncRelayCommand ResumeCommandAsync { get; set; }
        public RelayCommand PauseCommand { get; set; }
        public AsyncRelayCommand CancelCommandAsync { get; set; }

        private readonly GrpcService _grpcService;
        private readonly IDbContextFactory<MyDbContext> _dbContextFactory;
        private readonly HttpRequest _httpRequest;
        private readonly IMapper _mapper;
        public DownloadItemViewModel(GrpcService grpcService, IDbContextFactory<MyDbContext> dbContextFactory, HttpRequest httpRequest,
                                     IMapper mapper)
        {
            _mapper = mapper;
            _grpcService = grpcService;
            _httpRequest = httpRequest;
            _dbContextFactory = dbContextFactory;
            PauseCommand = new RelayCommand(Pause);
            CancelCommandAsync = new AsyncRelayCommand(CancelAsync);
            ResumeCommandAsync = new AsyncRelayCommand(ResumeAsync);
        }

        public async Task StartAsync()
        {
            _pauseTokenSource = new CancellationTokenSource();
            _cancelTokenSource = new CancellationTokenSource();
            _downloadTask = Task.Run(async () =>
            {
                await UpdateStatusAsync(FileDownloadingStatus.Downloading);
                Message = null;
                await _grpcService
                    .DownloadFileAsync(_pauseTokenSource.Token, _cancelTokenSource.Token, 
                    TempFileLocation, TaskId, TransferBytes, OnComplete, OnPause, OnError, OccurException, SetTransferBytes);
            });

            CanSuspend = true;
            await _downloadTask;
        }

        private async Task ResumeAsync()
        {
            if (!File.Exists(TempFileLocation))
                return;

            await StartAsync();
        }

        private void Pause()
        {
            _pauseTokenSource?.Cancel();
        }

        private async void OnPause() 
        {
            Message = "下载暂停";
            await UpdateStatusAsync(FileDownloadingStatus.DownloadingSuspend);
            CanSuspend = false;
        }

        private async Task CancelAsync()
        {
            await _httpRequest.PutAsync(string.Format(Const.CANCEL_DOWNLOADING, TaskId), null);
            _cancelTokenSource?.Cancel();
            Message = "取消下载";
            await UpdateStatusAsync(FileDownloadingStatus.DownloadingComplete);
            WeakReferenceMessenger.Default.Send<DownloadItemViewModel, string>(this, Const.DownloadFinish);
            if (await ExpressionWaiter.WaitInTime(() => _downloadTask == null || _downloadTask.IsCompleted))
            {
                if (File.Exists(TempFileLocation))
                {
                    File.Delete(TempFileLocation);
                }
            }
        }

        private async Task UpdateStatusAsync(FileDownloadingStatus fileStatus,string fileLocation = null)
        {
            using (var context = _dbContextFactory.CreateDbContext())
            {
                var record = await context.DownloadFileRecords.FirstOrDefaultAsync(x => x.Id == Id);
                if (record != null)
                {
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

                        await context.TransferCompleteRecords.AddAsync(transferRecord);
                    }

                    if (fileStatus == FileDownloadingStatus.DownloadingSuspend)
                    {
                        record.TransferBytes = TransferBytes;
                    }

                    await context.SaveChangesAsync();
                }
            }

            Status = fileStatus;
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
                    WeakReferenceMessenger.Default.Send<DownloadItemViewModel, string>(this, Const.DownloadFinish);

                    return;
                }
            }

            //文件改名
            var fileLocation = FileName.GetAvailableFileLocation(new FileInfo(TempFileLocation).DirectoryName);
            File.Move(TempFileLocation, fileLocation);

            await UpdateStatusAsync(FileDownloadingStatus.DownloadingComplete, fileLocation);
            WeakReferenceMessenger.Default.Send<DownloadItemViewModel, string>(this, Const.DownloadFinish);
        }
    }
}