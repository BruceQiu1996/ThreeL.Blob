using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Threading.Channels;
using ThreeL.Blob.Domain.Aggregate.FileObject;
using ThreeL.Blob.Infra.Repository.IRepositories;
using Image = SixLabors.ImageSharp.Image;

namespace ThreeL.Blob.Application.Channels
{
    public class GenerateThumbnailChannel
    {
        private readonly ChannelWriter<(long, long)> _writeChannel;
        private readonly ChannelReader<(long, long)> _readChannel;
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private readonly IServiceProvider _provider;
        private readonly IConfiguration _configuration;

        public GenerateThumbnailChannel(IServiceProvider provider, IConfiguration configuration)
        {
            _provider = provider;
            _configuration = configuration;
            var channel = Channel.CreateUnbounded<(long, long)>();
            _writeChannel = channel.Writer;
            _readChannel = channel.Reader;
            MessageCustomer readOperateLogService = new MessageCustomer(_readChannel,
                _provider.GetRequiredService<IEfBasicRepository<FileObject, long>>(), _provider.GetRequiredService<ILogger<MessageCustomer>>(), _configuration);

            Task.Run(async () => await readOperateLogService.StartAsync(_cancellationTokenSource.Token));
        }

        ~GenerateThumbnailChannel()
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
        }

        public async Task WriteMessageAsync((long, long) fileId)
        {
            await _writeChannel.WriteAsync(fileId);
        }

        public class MessageCustomer
        {
            private readonly ChannelReader<(long, long)> _readChannel;
            private readonly IEfBasicRepository<FileObject, long> _fileBasicRepository;
            private readonly ILogger<MessageCustomer> _logger;
            private readonly IConfiguration _configuration;

            public MessageCustomer(ChannelReader<(long, long)> readChannel,
                                   IEfBasicRepository<FileObject, long> fileBasicRepository,
                                   ILogger<MessageCustomer> logger,
                                   IConfiguration configuration)
            {
                _logger = logger;
                _readChannel = readChannel;
                _configuration = configuration;
                _fileBasicRepository = fileBasicRepository;
            }

            public async Task StartAsync(CancellationToken cancellationToken)
            {
                while (await _readChannel.WaitToReadAsync(cancellationToken))
                {
                    var data = await _readChannel.ReadAsync();
                    try
                    {
                        var file = await _fileBasicRepository.GetAsync(data.Item2);
                        if (file == null && !File.Exists(file.Location)) continue;

                        var result = ImageValidateByStream(file.Location);
                        if (result.Item1)
                        {
                            var thumbLocation =  _configuration.GetSection("FileStorage:ThumbnailImagesLocation").Value;
                            if (!Directory.Exists(thumbLocation))
                            {
                                Directory.CreateDirectory(thumbLocation);
                            }

                            var newPath = Path.Combine(thumbLocation, data.Item1.ToString(),
                               $"{Path.GetFileNameWithoutExtension(file.Location)}_thumbnail{Path.GetExtension(file.Location)}");

                            var flag = CreateThumbnail(file.Location, newPath, 100, 120);
                            if (flag)
                            {
                                file.ThumbnailImageLocation = newPath;
                                await _fileBasicRepository.UpdateAsync(file);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex.StackTrace);
                        continue;
                    }
                    if (cancellationToken.IsCancellationRequested) break;
                }
            }

            public Task StopAsync(CancellationToken cancellationToken)
            {
                return Task.CompletedTask;
            }

            public (bool, ImageType) ImageValidateByStream(string filePath)
            {
                using (var fileStream = File.OpenRead(filePath))
                using (BinaryReader br = new BinaryReader(fileStream))
                {
                    int length = 20;
                    StringBuilder stringBuilder = new StringBuilder();
                    while (length > 0)
                    {
                        byte tempByte = br.ReadByte();
                        stringBuilder.Append(Convert.ToString(tempByte, 16));
                        stringBuilder.Append(",");
                        length--;
                    }
                    string fileTypeString = stringBuilder.ToString().ToUpper();
                    if (string.IsNullOrEmpty(fileTypeString))
                        return (false, ImageType.Error);

                    if (fileTypeString.StartsWith("FF,D8,"))
                        return (true, ImageType.JPEG);
                    if (fileTypeString.StartsWith("89,50,4E,47,D,A,1A,A,"))
                        return (true, ImageType.PNG);
                    if (fileTypeString.StartsWith("42,4D,"))
                        return (true, ImageType.JPEG);
                    if (fileTypeString.StartsWith("47,49,46,38,39,61,") || fileTypeString.StartsWith("47,49,46,38,37,61,"))
                        return (true, ImageType.GIF);
                    if (fileTypeString.StartsWith("4D,4D") || fileTypeString.StartsWith("49,49"))
                        return (true, ImageType.TIFF);
                    if (fileTypeString.StartsWith("46,4F,52,4D"))
                        return (true, ImageType.TIFF);
                    return (false, ImageType.Empty);
                }
            }

            public enum ImageType
            {
                Error,
                Empty,
                JPEG,
                BMP,
                PNG,
                GIF,
                TIFF,
                IFF
            }

            public bool CreateThumbnail(string sourcePath, string newPath, int width, int height)
            {
                Image imageFrom = null;
                try
                {
                    imageFrom = Image.Load(sourcePath);
                    if (imageFrom.Width <= width && imageFrom.Height <= height)
                    {
                        imageFrom.Save(newPath);
                        imageFrom.Dispose();

                        return true;
                    }

                    int imageFromWidth = imageFrom.Width;
                    int imageFromHeight = imageFrom.Height;

                    float scale = height / (float)imageFromHeight;

                    if ((width / (float)imageFromWidth) < scale)
                        scale = width / (float)imageFromWidth;

                    width = (int)(imageFromWidth * scale);
                    height = (int)(imageFromHeight * scale);
                    imageFrom.Mutate(x => x.Resize(width, height));
                    if (!Directory.Exists(Path.GetDirectoryName(newPath)))
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(newPath)!);
                    }
                    imageFrom.Save(newPath);

                    return true;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.StackTrace);

                    return false;
                }
                finally
                {
                    imageFrom?.Dispose();
                }
            }
        }
    }
}
