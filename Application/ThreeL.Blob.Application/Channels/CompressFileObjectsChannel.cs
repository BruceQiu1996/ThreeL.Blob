using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.IO.Compression;
using System.Threading.Channels;
using ThreeL.Blob.Domain.Aggregate.FileObject;
using ThreeL.Blob.Infra.Core.Extensions.System;
using ThreeL.Blob.Infra.Repository.IRepositories;
using ThreeL.Blob.Shared.Domain.Metadata.FileObject;

namespace ThreeL.Blob.Application.Channels
{
    public class CompressFileObjectsChannel
    {
        private readonly ChannelWriter<(string, string, long, long, FileObject[])> _writeChannel;
        private readonly ChannelReader<(string, string, long, long, FileObject[])> _readChannel;
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private readonly IServiceProvider _provider;

        public CompressFileObjectsChannel(IServiceProvider provider)
        {
            _provider = provider;
            var channel = Channel.CreateUnbounded<(string, string, long, long, FileObject[])>();
            _writeChannel = channel.Writer;
            _readChannel = channel.Reader;
            MessageCustomer readOperateLogService = new MessageCustomer(_readChannel,
                _provider.GetRequiredService<ILogger<MessageCustomer>>(), _provider.GetRequiredService<IEfBasicRepository<FileObject, long>>());

            Task.Run(async () => await readOperateLogService.StartAsync(_cancellationTokenSource.Token));
        }

        ~CompressFileObjectsChannel()
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
        }

        public async Task WriteMessageAsync((string, string, long, long, FileObject[]) task)
        {
            await _writeChannel.WriteAsync(task);
        }

        public class MessageCustomer
        {
            private readonly ChannelReader<(string, string, long, long, FileObject[])> _readChannel;
            private readonly ILogger<MessageCustomer> _logger;
            private readonly IEfBasicRepository<FileObject, long> _efBasicRepository;

            public MessageCustomer(ChannelReader<(string, string, long, long, FileObject[])> readChannel,
                                   ILogger<MessageCustomer> logger,
                                   IEfBasicRepository<FileObject, long> efBasicRepository)
            {
                _logger = logger;
                _readChannel = readChannel;
                _efBasicRepository = efBasicRepository;
            }

            public async Task StartAsync(CancellationToken cancellationToken)
            {
                while (await _readChannel.WaitToReadAsync(cancellationToken))
                {
                    while (_readChannel.TryRead(out var temp))
                    {
                        var folderLocation = Path.Combine(temp.Item2, Guid.NewGuid().ToString());
                        if (!Directory.Exists(folderLocation))
                            Directory.CreateDirectory(folderLocation);
                        try
                        {

                            foreach (var item in temp.Item5)
                            {
                                if (item.IsFolder && Directory.Exists(item.Location))
                                {
                                    CopyFolder(folderLocation, new DirectoryInfo(item.Location));
                                }
                                else if (!item.IsFolder && File.Exists(item.Location))
                                {
                                    File.Copy(item.Location, Path.Combine(folderLocation, item.Name));
                                }
                            }

                            var target = Path.Combine(temp.Item2, $"{Guid.NewGuid()}.zip");
                            ZipFile.CreateFromDirectory(folderLocation, target);
                            //创建新的文件数据
                            var fileObj = new FileObject();
                            fileObj.CreateBy = temp.Item4;
                            fileObj.CreateTime = DateTime.Now;
                            fileObj.UploadFinishTime = DateTime.Now;
                            fileObj.LastUpdateTime = DateTime.Now;
                            fileObj.Status = FileStatus.Normal;
                            fileObj.Location = target;
                            fileObj.ParentFolder = temp.Item3;
                            using (var fs = new FileStream(target, FileMode.Open))
                            {
                                fileObj.Code = fs.ToSHA256();
                                fileObj.Size = fs.Length;
                                fileObj.Name = fs.Name;
                            }

                            await _efBasicRepository.InsertAsync(fileObj);

                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex.ToString());
                            continue;
                        }
                        finally
                        {
                            if (Directory.Exists(folderLocation))
                                Directory.Delete(folderLocation, true);
                        }
                    }

                    if (cancellationToken.IsCancellationRequested) break;
                }
            }

            /// <summary>
            /// 复制文件夹
            /// </summary>
            /// <param name="parentLocationFolder"></param>
            /// <param name="directoryInfo"></param>
            private void CopyFolder(string parentLocationFolder, DirectoryInfo directoryInfo)
            {
                var location = Path.Combine(parentLocationFolder, directoryInfo.Name);
                if (!Directory.Exists(location))
                    Directory.CreateDirectory(location);

                foreach (var file in directoryInfo.GetFiles())
                {
                    try
                    {
                        file.CopyTo(Path.Combine(location, file.Name));
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex.ToString());
                        continue;
                    }
                }

                foreach (var directory in directoryInfo.GetDirectories())
                {
                    CopyFolder(directoryInfo.FullName, directory);
                }
            }

            public Task StopAsync(CancellationToken cancellationToken)
            {
                return Task.CompletedTask;
            }
        }

    }
}
