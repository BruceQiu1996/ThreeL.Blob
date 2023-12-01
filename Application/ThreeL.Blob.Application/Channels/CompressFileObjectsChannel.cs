using ICSharpCode.SharpZipLib.Zip;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.IO.Compression;
using System.Threading.Channels;
using ThreeL.Blob.Domain.Aggregate.FileObject;

namespace ThreeL.Blob.Application.Channels
{
    public class CompressFileObjectsChannel
    {
        private readonly ChannelWriter<(string, FileObject[])> _writeChannel;
        private readonly ChannelReader<(string, FileObject[])> _readChannel;
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private readonly IServiceProvider _provider;

        public CompressFileObjectsChannel(IServiceProvider provider)
        {
            _provider = provider;
            var channel = Channel.CreateUnbounded<(string, FileObject[])>();
            _writeChannel = channel.Writer;
            _readChannel = channel.Reader;
            MessageCustomer readOperateLogService = new MessageCustomer(_readChannel, _provider.GetRequiredService<ILogger<MessageCustomer>>());

            Task.Run(async () => await readOperateLogService.StartAsync(_cancellationTokenSource.Token));
        }

        ~CompressFileObjectsChannel()
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
        }

        public async Task WriteMessageAsync((string, FileObject[]) task)
        {
            await _writeChannel.WriteAsync(task);
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
                    while (_readChannel.TryRead(out var temp))
                    {
                        try
                        {
                            Directory.CreateDirectory(temp.Item1);
                            foreach (var item in temp.Item2)
                            {
                                if (item.IsFolder && Directory.Exists(item.Location)) 
                                {
                                    Directory.Move(item.Location,Path.Combine(temp.Item1,Path.GetDirectoryName(item.Location)));
                                }
                                else if(!item.IsFolder && File.Exists(item.Location))
                                {
                                    File.Move(item.Location,Path.Combine(temp.Item1,Path.GetFileName(item.Location)));
                                }
                            }

                            using (FileStream zipFile = File.Create(zipedFile))
                            {
                                using (ZipOutputStream s = new ZipOutputStream(ZipFile))
                                {
                                    s.SetLevel(9);
                                    ZipSetp(temp.Item1, s, "");
                                }
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

            private  void ZipSetp(string strDirectory, ZipOutputStream s, string parentPath, List<string> files = null)
            {
                if (strDirectory[strDirectory.Length - 1] != Path.DirectorySeparatorChar)
                {
                    strDirectory += Path.DirectorySeparatorChar;
                }

                string[] filenames = Directory.GetFileSystemEntries(strDirectory);

                byte[] buffer = new byte[4096];
                foreach (string file in filenames)
                {
                    if (files != null && !files.Contains(file))
                    {
                        continue;
                    }
                    if (Directory.Exists(file))
                    {
                        string pPath = Path.Combine(parentPath, Path.GetFileName(file));
                        ZipSetp(file, s, pPath, files);
                    }
                    else
                    {
                        string fileName = parentPath + Path.GetFileName(file);
                        ZipEntry entry = new ZipEntry(fileName);
                        entry.DateTime = DateTime.Now;
                        s.PutNextEntry(entry);
                        using (FileStream fs = File.OpenRead(file))
                        {
                            int sourceBytes;
                            do
                            {
                                sourceBytes = fs.Read(buffer, 0, buffer.Length);
                                s.Write(buffer, 0, sourceBytes);
                            } while (sourceBytes > 0);

                        }
                    }
                }
            }


            public Task StopAsync(CancellationToken cancellationToken)
            {
                return Task.CompletedTask;
            }
        }

    }
}
