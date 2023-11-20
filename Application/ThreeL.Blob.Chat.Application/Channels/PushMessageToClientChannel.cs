using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Threading.Channels;

namespace ThreeL.Blob.Chat.Application.Channels
{
    /// <summary>
    /// 推送消息的channel
    /// </summary>
    public class PushMessageToClientChannel
    {
        private readonly ChannelWriter<(long id, string topic, object body)> _writeChannel;
        private readonly ChannelReader<(long id, string topic, object body)> _readChannel;
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private readonly IServiceProvider _provider;
        public Action<(long id, string topic, object body)> MessageHandler;

        public PushMessageToClientChannel(IServiceProvider provider)
        {
            _provider = provider;
            var channel = Channel.CreateUnbounded<(long id, string topic, object body)>();
            _writeChannel = channel.Writer;
            _readChannel = channel.Reader;
            MessageCustomer readOperateLogService = new MessageCustomer(_readChannel, _provider.GetRequiredService<ILogger<MessageCustomer>>(), MessageHandler);

            Task.Run(async () => await readOperateLogService.StartAsync(_cancellationTokenSource.Token));
        }

        ~PushMessageToClientChannel()
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
        }

        public async Task WriteMessageAsync((long id, string topic, object body) message)
        {
            await _writeChannel.WriteAsync(message);
        }

        public class MessageCustomer
        {
            private readonly ChannelReader<(long id, string topic, object body)> _readChannel;
            private readonly ILogger<MessageCustomer> _logger;
            private readonly Action<(long id, string topic, object body)> _handler;

            public MessageCustomer(ChannelReader<(long id, string topic, object body)> readChannel,
                                   ILogger<MessageCustomer> logger,
                                   Action<(long id, string topic, object body)> handler)
            {
                _handler = handler;
                _logger = logger;
                _readChannel = readChannel;
            }

            public async Task StartAsync(CancellationToken cancellationToken)
            {
                while (await _readChannel.WaitToReadAsync(cancellationToken))
                {
                    while (_readChannel.TryRead(out var message))
                    {
                        try
                        {
                            _handler?.Invoke(message);
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
