using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Threading.Channels;
using ThreeL.Blob.Domain.Aggregate.FileObject;

namespace ThreeL.Blob.Application.Channels
{
    /// <summary>
    /// 删除文件的channel
    /// </summary>
    public class DeleteFilesChannel
    {
        private readonly ChannelWriter<FileObject> _writeChannel;
        private readonly ChannelReader<FileObject> _readChannel;
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private readonly IServiceProvider _provider;

        public DeleteFilesChannel(IServiceProvider provider)
        {
            _provider = provider;
            var channel = Channel.CreateUnbounded<FileObject>();
            _writeChannel = channel.Writer;
            _readChannel = channel.Reader;
            MessageCustomer readOperateLogService = new MessageCustomer(_readChannel, _provider.GetRequiredService<ILogger<MessageCustomer>>());

            Task.Run(async () => await readOperateLogService.StartAsync(_cancellationTokenSource.Token));
        }

        ~DeleteFilesChannel()
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
        }

        public async Task WriteMessageAsync(IEnumerable<FileObject> files)
        {
            foreach (var file in files.Where(x => !x.IsFolder)) //先删除文件
            {
                await _writeChannel.WriteAsync(file);
            }

            foreach (var file in files.Where(x => x.IsFolder))
            {
                await _writeChannel.WriteAsync(file);
            }
        }

        public class MessageCustomer
        {
            private readonly ChannelReader<FileObject> _readChannel;
            private readonly ILogger<MessageCustomer> _logger;

            public MessageCustomer(ChannelReader<FileObject> readChannel,
                                   ILogger<MessageCustomer> logger)
            {
                _logger = logger;
                _readChannel = readChannel;
            }

            public async Task StartAsync(CancellationToken cancellationToken)
            {
                while (await _readChannel.WaitToReadAsync(cancellationToken))
                {
                    while (_readChannel.TryRead(out var FileObject))
                    {
                        try
                        {
                            if (File.Exists(FileObject.Location))
                            {
                                File.Delete(FileObject.Location);
                                _logger.LogInformation($"用户{FileObject.CreateBy},删除文件成功:{FileObject.Location}");
                            }

                            if (File.Exists(FileObject.TempFileLocation))
                            {
                                File.Delete(FileObject.TempFileLocation);
                                _logger.LogInformation($"用户{FileObject.CreateBy},删除临时文件成功:{FileObject.Location}");
                            }

                            if (File.Exists(FileObject.ThumbnailImageLocation))
                            {
                                File.Delete(FileObject.ThumbnailImageLocation);
                                _logger.LogInformation($"用户{FileObject.CreateBy},删除缩略图成功:{FileObject.Location}");
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex.ToString());
                            continue;
                        }
                    }

                    if (cancellationToken.IsCancellationRequested) break;
                }
            }

            public Task StopAsync(CancellationToken cancellationToken)
            {
                return Task.CompletedTask;
            }
        }
    }
}
