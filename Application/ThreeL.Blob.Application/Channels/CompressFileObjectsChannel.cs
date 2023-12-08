using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.IO.Compression;
using System.Threading.Channels;
using ThreeL.Blob.Domain.Aggregate.FileObject;
using ThreeL.Blob.Infra.Repository.IRepositories;

namespace ThreeL.Blob.Application.Channels
{
    public class CompressFileObjectsChannel
    {
        private readonly ChannelWriter<(string zipName, string rootLocation, long rootId, long userId, FileObject[] items)> _writeChannel;
        private readonly ChannelReader<(string zipName, string rootLocation, long rootId, long userId, FileObject[] items)> _readChannel;
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private readonly IServiceProvider _provider;

        public CompressFileObjectsChannel(IServiceProvider provider)
        {
            _provider = provider;
            var channel = Channel.CreateUnbounded<(string zipName, string rootLocation, long rootId, long userId, FileObject[] items)>();
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

        public async Task WriteMessageAsync((string zipName, string rootLocation, long rootId, long userId, FileObject[] items) task)
        {
            await _writeChannel.WriteAsync(task);
        }

        public class MessageCustomer
        {
            private readonly ChannelReader<(string zipName, string rootLocation, long rootId, long userId, FileObject[] items)> _readChannel;
            private readonly ILogger<MessageCustomer> _logger;
            private readonly IEfBasicRepository<FileObject, long> _efBasicRepository;

            public MessageCustomer(ChannelReader<(string zipName, string rootLocation, long rootId, long userId, FileObject[] items)> readChannel,
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
                        var newFolder = Path.Combine(temp.rootLocation, Guid.NewGuid().ToString());
                        Directory.CreateDirectory(newFolder);
                        foreach (var item in temp.items)
                        {
                            await CopyFileObjectsToFolderAsync(newFolder, item, temp.userId);
                        }

                        ZipFile.CreateFromDirectory(newFolder, Path.Combine(temp.rootLocation, $"{temp.zipName}.zip"));
                    }

                    if (cancellationToken.IsCancellationRequested) break;
                }
            }

            public async ValueTask CopyFileObjectsToFolderAsync(string parentLocation, FileObject file, long userId)
            {
                if (!file.IsFolder && File.Exists(file.Location))
                {
                    File.Copy(file.Location, Path.Combine(parentLocation, file.Name), true);
                }
                else if (file.IsFolder && Directory.Exists(file.Location))
                {
                    var newFolderLocation = Path.Combine(parentLocation, file.Name);
                    Directory.CreateDirectory(newFolderLocation);
                    var items = await _efBasicRepository
                        .Where(x => x.ParentFolder == file.Id && x.CreateBy == userId).ToListAsync();

                    foreach (var item in items)
                    {
                        await CopyFileObjectsToFolderAsync(newFolderLocation, item, userId);
                    }
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
