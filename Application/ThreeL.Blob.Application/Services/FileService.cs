using AutoMapper;
using StackExchange.Redis;
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
    public class FileService : IFileService
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
            }else
                location = user.; //用户根目录

            var fileObj = _mapper.Map<FileObject>(uploadFileDto);
            fileObj.CreateBy = userId;
            fileObj.CreateTime = DateTime.UtcNow;
            fileObj.Status = FileStatus.Wait;

            await _fileBasicRepository.InsertAsync(fileObj);
            await _redisProvider.HSetAsync($"{Const.REDIS_UPLOADFILE_CACHE_KEY}{userId}", fileObj.Id.ToString(), fileObj);
        }
    }
}
