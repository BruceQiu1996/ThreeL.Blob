using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Net;
using ThreeL.Blob.Application.Contract.Configurations;
using ThreeL.Blob.Application.Contract.Dtos;
using ThreeL.Blob.Application.Contract.Services;
using ThreeL.Blob.Domain.Aggregate.FileObject;
using ThreeL.Blob.Domain.Aggregate.User;
using ThreeL.Blob.Infra.Redis;
using ThreeL.Blob.Infra.Repository.IRepositories;
using ThreeL.Blob.Shared.Application.Contract.Services;
using ThreeL.Blob.Shared.Domain.Metadata.FileObject;

namespace ThreeL.Blob.Application.Services
{
    public class FileService : IFileService, IAppService
    {
        private readonly IEfBasicRepository<User, long> _userBasicRepository;
        private readonly IEfBasicRepository<FileObject, long> _fileBasicRepository;
        private readonly IRedisProvider _redisProvider;
        private readonly IMapper _mapper;
        public FileService(IEfBasicRepository<User, long> userBasicRepository,
                           IEfBasicRepository<FileObject, long> fileBasicRepository,
                           IRedisProvider redisProvider,
                           IMapper mapper)
        {
            _mapper = mapper;
            _redisProvider = redisProvider;
            _userBasicRepository = userBasicRepository;
            _fileBasicRepository = fileBasicRepository;
        }

        public async Task<ServiceResult<IEnumerable<FileObjDto>>> GetItemsAsync(long parentId, long userId)
        {
            var items = await _fileBasicRepository.Where(x => x.ParentFolder == parentId && x.CreateBy == userId && x.Status == FileStatus.Normal)
                .OrderByDescending(x => x.CreateTime).ToListAsync();

            return new ServiceResult<IEnumerable<FileObjDto>>(items.Select(_mapper.Map<FileObjDto>));
        }

        public async Task<ServiceResult<UploadFileResponseDto>> UploadAsync(UploadFileDto uploadFileDto, long userId)
        {
            var user = await _userBasicRepository.GetAsync(userId);
            if (user == null)
                return new ServiceResult<UploadFileResponseDto>(HttpStatusCode.BadRequest, "用户未授权");

            string location = null;
            if (uploadFileDto.ParentFolder != default)
            {
                var folder = await _fileBasicRepository.FirstOrDefaultAsync(x => x.IsFolder && x.Id == uploadFileDto.ParentFolder);
                if (folder == null)
                    return new ServiceResult<UploadFileResponseDto>(HttpStatusCode.BadRequest, "文件夹不存在");

                location = folder.Location!;
            }
            else
                location = user.Location;

            if (!Directory.Exists(location))
                return new ServiceResult<UploadFileResponseDto>(HttpStatusCode.BadRequest, "数据异常");

            var tempFileName = Path.Combine(location, $"{Path.GetRandomFileName()}.tmp");
            File.Create(tempFileName).Close();
            var fileObj = _mapper.Map<FileObject>(uploadFileDto);
            fileObj.CreateBy = userId;
            fileObj.CreateTime = DateTime.UtcNow;
            fileObj.Status = FileStatus.Wait;
            fileObj.TempFileLocation = tempFileName;

            await _fileBasicRepository.InsertAsync(fileObj);
            await _redisProvider.HSetAsync($"{Const.REDIS_UPLOADFILE_CACHE_KEY}{userId}", fileObj.Id.ToString(), fileObj, TimeSpan.FromDays(3));

            return new ServiceResult<UploadFileResponseDto>(new UploadFileResponseDto()
            {
                FileId = fileObj.Id
            });
        }
    }
}
