using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Threading.Channels;

namespace ThreeL.Blob.Application.Channels
{
    public class CallChatGrpcChannel
    {
        private readonly ChannelWriter<Action> _writeChannel;
        private readonly ChannelReader<Action> _readChannel;
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private readonly IServiceProvider _provider;

        public CallChatGrpcChannel(IServiceProvider provider)
        {
            _provider = provider;
            var channel = Channel.CreateUnbounded<Action>();
            _writeChannel = channel.Writer;
            _readChannel = channel.Reader;
            MessageCustomer readOperateLogService = new MessageCustomer(_readChannel, _provider.GetRequiredService<ILogger<MessageCustomer>>());

            Task.Run(async () => await readOperateLogService.StartAsync(_cancellationTokenSource.Token));
        }

        ~CallChatGrpcChannel()
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
        }

        public async Task WriteMessageAsync(Action file)
        {
            await _writeChannel.WriteAsync(file);
        }

        public class MessageCustomer
        {
            private readonly ChannelReader<Action> _readChannel;
            private readonly ILogger<MessageCustomer> _logger;

            public MessageCustomer(ChannelReader<Action> readChannel,
                                   ILogger<MessageCustomer> logger)
            {
                _logger = logger;
                _readChannel = readChannel;
            }

            public async Task StartAsync(CancellationToken cancellationToken)
            {
                while (await _readChannel.WaitToReadAsync(cancellationToken))
                {
                    while (_readChannel.TryRead(out var task))
                    {
                        try
                        {
                            task.Invoke();
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
