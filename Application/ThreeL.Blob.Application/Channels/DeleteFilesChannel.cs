using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Threading.Channels;
using ThreeL.Blob.Domain.Aggregate.(string, FileObject[]);

namespace ThreeL.Blob.Application.Channels
{
    /// <summary>
    /// 删除文件的channel
    /// </summary>
    public class DeleteFilesChannel
    {
        private readonly ChannelWriter<(string, FileObject[])> _writeChannel;
        private readonly ChannelReader<(string, FileObject[])> _readChannel;
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private readonly IServiceProvider _provider;

        public DeleteFilesChannel(IServiceProvider provider)
        {
            _provider = provider;
            var channel = Channel.CreateUnbounded<(string, FileObject[])>();
            _writeChannel = channel.Writer;
            _readChannel = channel.Reader;
            MessageCustomer readOperateLogService = new MessageCustomer(_readChannel,_provider.GetRequiredService<ILogger<MessageCustomer>>());

            Task.Run(async () => await readOperateLogService.StartAsync(_cancellationTokenSource.Token));
        }

        ~DeleteFilesChannel()
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
        }

        public async Task WriteMessageAsync(IEnumerable<(string, FileObject[])> files)
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
            private readonly ChannelReader<(string, FileObject[])> _readChannel;
            private readonly ILogger<MessageCustomer> _logger;

            public MessageCustomer(ChannelReader<(string, FileObject[])> readChannel,
                                   ILogger<MessageCustomer> logger)
            {
                _logger = logger;
                _readChannel = readChannel;
            }

            public async Task StartAsync(CancellationToken cancellationToken)
            { 
                while (await _readChannel.WaitToReadAsync(cancellationToken))
                {
                    while (_readChannel.TryRead(out var (string, FileObject[])))
                    {
                        try
                        {
                            if (File.Exists((string, FileObject[]).Location)) 
                            {
                                File.Delete((string, FileObject[]).Location);
                                _logger.LogInformation($"用户{(string, FileObject[]).CreateBy},删除文件成功:{(string, FileObject[]).Location}");
                            }

                            if (File.Exists((string, FileObject[]).TempFileLocation))
                            {
                                File.Delete((string, FileObject[]).TempFileLocation);
                                _logger.LogInformation($"用户{(string, FileObject[]).CreateBy},删除临时文件成功:{(string, FileObject[]).Location}");
                            }

                            if (File.Exists((string, FileObject[]).ThumbnailImageLocation))
                            {
                                File.Delete((string, FileObject[]).ThumbnailImageLocation);
                                _logger.LogInformation($"用户{(string, FileObject[]).CreateBy},删除缩略图成功:{(string, FileObject[]).Location}");
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
