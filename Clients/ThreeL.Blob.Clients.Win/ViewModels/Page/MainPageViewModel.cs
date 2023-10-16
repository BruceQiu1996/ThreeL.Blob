using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using ThreeL.Blob.Clients.Win.Dtos;
using ThreeL.Blob.Clients.Win.Entities;
using ThreeL.Blob.Clients.Win.Helpers;
using ThreeL.Blob.Clients.Win.Request;
using ThreeL.Blob.Clients.Win.Resources;
using ThreeL.Blob.Infra.Core.Extensions.System;
using ThreeL.Blob.Infra.Core.Serializers;

namespace ThreeL.Blob.Clients.Win.ViewModels.Page
{
    public class MainPageViewModel : ObservableObject
    {
        public AsyncRelayCommand UploadCommandAsync { get; set; }
        private readonly GrpcService _grpcService;
        private readonly HttpRequest _httpRequest;
        private readonly IDbContextFactory<MyDbContext> _dbContextFactory;
        private readonly GrowlHelper _growlHelper;
        public MainPageViewModel(GrpcService grpcService, HttpRequest httpRequest, IDbContextFactory<MyDbContext> dbContextFactory,
                                 GrowlHelper growlHelper)
        {
            _grpcService = grpcService;
            _httpRequest = httpRequest;
            _growlHelper = growlHelper;
            _dbContextFactory = dbContextFactory;
            UploadCommandAsync = new AsyncRelayCommand(UploadAsync);
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
