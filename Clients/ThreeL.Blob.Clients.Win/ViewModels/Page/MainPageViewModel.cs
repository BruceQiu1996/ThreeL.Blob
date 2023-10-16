using AutoMapper;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Win32;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using ThreeL.Blob.Clients.Win.Dtos;
using ThreeL.Blob.Clients.Win.Entities;
using ThreeL.Blob.Clients.Win.Helpers;
using ThreeL.Blob.Clients.Win.Request;
using ThreeL.Blob.Clients.Win.Resources;
using ThreeL.Blob.Clients.Win.ViewModels.Item;
using ThreeL.Blob.Infra.Core.Extensions.System;
using ThreeL.Blob.Infra.Core.Serializers;

namespace ThreeL.Blob.Clients.Win.ViewModels.Page
{
    public class MainPageViewModel : ObservableObject
    {
        public AsyncRelayCommand UploadCommandAsync { get; set; }
        public AsyncRelayCommand LoadCommandAsync { get; set; }
        public AsyncRelayCommand RefreshCommandAsync { get; set; }
        private readonly GrpcService _grpcService;
        private readonly HttpRequest _httpRequest;
        private readonly IDbContextFactory<MyDbContext> _dbContextFactory;
        private readonly GrowlHelper _growlHelper;
        private readonly IMapper _mapper;
        private long _currentParent = 0;

        private ObservableCollection<FileObjItemViewModel> _fileObjDtos;
        public ObservableCollection<FileObjItemViewModel> FileObjDtos
        {
            get => _fileObjDtos;
            set => SetProperty(ref _fileObjDtos, value);
        }

        public MainPageViewModel(GrpcService grpcService, HttpRequest httpRequest, IDbContextFactory<MyDbContext> dbContextFactory,
                                 GrowlHelper growlHelper, IMapper mapper)
        {
            _mapper = mapper;
            _grpcService = grpcService;
            _httpRequest = httpRequest;
            _growlHelper = growlHelper;
            _dbContextFactory = dbContextFactory;
            UploadCommandAsync = new AsyncRelayCommand(UploadAsync);
            LoadCommandAsync = new AsyncRelayCommand(LoadAsync);
            RefreshCommandAsync = new AsyncRelayCommand(RefreshAsync);
            FileObjDtos = new ObservableCollection<FileObjItemViewModel>();
        }

        private async Task LoadAsync() 
        {
            await RefreshByParentAsync(0);
        }

        private async Task RefreshAsync() 
        {
            await RefreshByParentAsync(_currentParent);
        }

        private async Task RefreshByParentAsync(long parent) 
        {
            var resp = await _httpRequest.GetAsync($"{Const.UPLOAD_FILE}/{parent}");
            if (resp != null)
            {
                var items = JsonSerializer
                    .Deserialize<IEnumerable<FileObjDto>>(await resp.Content.ReadAsStringAsync(), SystemTextJsonSerializer.GetDefaultOptions());

                if (items != null && items.Count() > 0)
                {
                    FileObjDtos = new ObservableCollection<FileObjItemViewModel>(items.Select(_mapper.Map<FileObjItemViewModel>));
                }
            }
        }

        private async Task UploadAsync()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog()!.Value)
            {
                var fileInfo = new FileInfo(openFileDialog.FileName);
                using (FileStream stream = new FileStream(openFileDialog.FileName, FileMode.Open, FileAccess.Read))
                {
                    var code = stream.ToSHA256();
                    var resp = await _httpRequest.PostAsync(Const.UPLOAD_FILE, new UploadFileDto()
                    {
                        Name = fileInfo.Name,
                        Size = fileInfo.Length,
                        ParentFolder = 0,
                        Code = code
                    });

                    if (resp != null && resp.IsSuccessStatusCode)
                    {
                        var result = JsonSerializer.
                            Deserialize<UploadFileResponseDto>(await resp.Content.ReadAsStringAsync(), SystemTextJsonSerializer.GetDefaultOptions());
                        using (var context = await _dbContextFactory.CreateDbContextAsync())
                        {
                            var record = new UploadFileRecord()
                            {
                                FileId = result.FileId,
                                FileName = fileInfo.Name,
                                Size = fileInfo.Length,
                                FileLocation = fileInfo.FullName,
                                TransferBytes = 0,
                                Status = Status.Doing,
                                Code = code
                            };
                            await context.UploadFileRecords.AddAsync(record);
                            await context.SaveChangesAsync();
                            WeakReferenceMessenger.Default.Send<UploadFileRecord, string>(record, Const.AddUploadRecord);
                        };
                    }
                }
            }
        }
    }
}
