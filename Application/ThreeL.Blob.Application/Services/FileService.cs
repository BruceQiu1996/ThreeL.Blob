using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
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
        private readonly IEfBasicRepository<DownloadFileTask, string> _downloadTaskBasicRepository;
        private readonly IRedisProvider _redisProvider;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        public FileService(IEfBasicRepository<User, long> userBasicRepository,
                           IEfBasicRepository<FileObject, long> fileBasicRepository,
                           IEfBasicRepository<DownloadFileTask, string> downloadTaskBasicRepository,
                           IRedisProvider redisProvider,
                           IMapper mapper,
                           IConfiguration configuration)
        {
            _mapper = mapper;
            _redisProvider = redisProvider;
            _downloadTaskBasicRepository = downloadTaskBasicRepository;
            _userBasicRepository = userBasicRepository;
            _fileBasicRepository = fileBasicRepository;
            _configuration = configuration;
        }

        public async Task<ServiceResult> CancelDownloadingAsync(string taskId, long userId)
        {
            var task = await _downloadTaskBasicRepository.GetAsync(taskId);
            if (task == null || task.Status != DownloadTaskStatus.Downloading || task.CreateBy != userId)
            {
                return new ServiceResult(HttpStatusCode.BadRequest, "取消下载任务异常");
            }

            task.Status = DownloadTaskStatus.Cancelled;
            await _downloadTaskBasicRepository.UpdateAsync(task);

            return new ServiceResult();
        }

        public async Task<ServiceResult<FileUploadingStatusDto>> CancelUploadingAsync(long fileId, long userId)
        {
            var file = await _fileBasicRepository.FirstOrDefaultAsync(x => x.Id == fileId && x.CreateBy == userId);
            if (file == null)
                return new ServiceResult<FileUploadingStatusDto>(HttpStatusCode.BadRequest, "数据异常");

            file.Status = FileStatus.Cancel;
            file.UploadFinishTime = DateTime.UtcNow;
            await _fileBasicRepository.UpdateAsync(file);

            return new ServiceResult<FileUploadingStatusDto>
            (
                new FileUploadingStatusDto()
                {
                    Id = fileId,
                    Status = file.Status!.Value
                }
            );
        }

        public async Task<ServiceResult<FileObjDto>> CreateFolderAsync(FolderCreationDto folderCreationDto, long userId)
        {
            string parentFolderLocation = null;
            if (folderCreationDto.ParentId != 0)
            {
                var parentFolder = await _fileBasicRepository.GetAsync(folderCreationDto.ParentId);
                if (parentFolder == null || !parentFolder.IsFolder || !Directory.Exists(parentFolder.Location))
                {
                    return new ServiceResult<FileObjDto>(HttpStatusCode.BadRequest, "目录数据异常");
                }
                parentFolderLocation = parentFolder.Location;
            }
            else
            {
                var user = await _userBasicRepository.GetAsync(userId);
                if (user == null || !Directory.Exists(user.Location))
                {
                    return new ServiceResult<FileObjDto>(HttpStatusCode.BadRequest, "目录数据异常");
                }
                parentFolderLocation = user.Location;
            }

            var folderName = Guid.NewGuid().ToString();
            var folderLocation = Path.Combine(parentFolderLocation, folderName);
            Directory.CreateDirectory(folderLocation);
            var exist = await _fileBasicRepository.FirstOrDefaultAsync(x => x.IsFolder && x.Name == folderCreationDto.FolderName);
            if (exist != null)
            {
                folderCreationDto.FolderName = $"{folderCreationDto.FolderName}_{DateTime.Now.ToString("yyyyMMddhhmmssfff")}";
            }
            var fileObject = new FileObject()
            {
                CreateBy = userId,
                CreateTime = DateTime.UtcNow,
                LastUpdateTime = DateTime.UtcNow,
                UploadFinishTime = DateTime.UtcNow,
                IsFolder = true,
                Name = folderCreationDto.FolderName,
                ParentFolder = folderCreationDto.ParentId,
                Location = folderLocation,
                Status = FileStatus.Normal
            };
            await _fileBasicRepository.InsertAsync(fileObject);

            return new ServiceResult<FileObjDto>(_mapper.Map<FileObjDto>(fileObject));
        }

        //TODO删除逻辑完善
        public async Task<ServiceResult> DeleteItemsAsync(long[] fileIds, long userId)
        {
            List<FileObject> fileObjects = new List<FileObject>();
            var roots = await _fileBasicRepository.Where(x => fileIds.Contains(x.Id) && x.CreateBy == userId).ToListAsync();
            if (roots == null || roots.Count <= 0)
            {
                return new ServiceResult();
            }


            fileObjects.AddRange(roots.Where(x => !x.IsFolder));
            foreach (var root in roots.Where(x => x.IsFolder))
            {
                var files = await _fileBasicRepository
                    .QuerySqlAsync($"WITH RECURSIVE file_teee As(SELECT * FROM fileobject WHERE fileobject.Id = {root.Id} UNION SELECT f1.* FROM fileobject f1 JOIN file_teee ON f1.ParentFolder = file_teee.id) SELECT * FROM file_teee");
                fileObjects.AddRange(files);
            }

            return new ServiceResult();
        }

        public async Task<ServiceResult<DownloadFileResponseDto>> DownloadAsync(long fileId, long userId)
        {
            var file = await _fileBasicRepository.FirstOrDefaultAsync(x => x.Id == fileId && x.CreateBy == userId);
            if (file == null || file.IsFolder || !File.Exists(file.Location) || file.Status != FileStatus.Normal)
                return new ServiceResult<DownloadFileResponseDto>(HttpStatusCode.BadRequest, "数据异常");

            var task = new DownloadFileTask()
            {
                CreateTime = DateTime.UtcNow,
                FileId = fileId,
                CreateBy = userId,
                Status = DownloadTaskStatus.Wait,
            };

            await _downloadTaskBasicRepository.InsertAsync(task);
            await _redisProvider.HSetAsync($"{Const.REDIS_DOWNLOADTASK_CACHE_KEY}{userId}", task.Id, (long)0, TimeSpan.FromDays(3));

            return new ServiceResult<DownloadFileResponseDto>(new DownloadFileResponseDto()
            {
                FileId = fileId,
                Code = file.Code,
                TaskId = task.Id,
                Size = file.Size!.Value,
                FileName = file.Name
            });
        }

        public async Task<ServiceResult<IEnumerable<FileObjDto>>> GetItemsAsync(long parentId, long userId)
        {
            var items = await _fileBasicRepository
                .Where(x => x.ParentFolder == parentId && x.CreateBy == userId && x.Status == FileStatus.Normal)
                .OrderByDescending(x => x.CreateTime).ToListAsync();

            return new ServiceResult<IEnumerable<FileObjDto>>(items.Select(x =>
            {
                x.ThumbnailImageLocation = (string.IsNullOrEmpty(x.ThumbnailImageLocation) || !File.Exists(x.ThumbnailImageLocation)) ? null :
                    Path.GetFileName(x.ThumbnailImageLocation);

                return _mapper.Map<FileObjDto>(x);
            }));
        }

        public async Task<ServiceResult<FileUploadingStatusDto>> GetUploadingStatusAsync(long fileId, long userId)
        {
            var file = await _fileBasicRepository.FirstOrDefaultAsync(x => x.Id == fileId && x.CreateBy == userId);
            if (file == null)
                return new ServiceResult<FileUploadingStatusDto>(HttpStatusCode.BadRequest, "数据异常");

            var status = await _redisProvider
                    .HGetAsync($"{Const.REDIS_UPLOADFILE_CACHE_KEY}{userId}", fileId.ToString());
            if (status == null)
            {
                return new ServiceResult<FileUploadingStatusDto>(HttpStatusCode.BadRequest, "数据异常");
            }

            return new ServiceResult<FileUploadingStatusDto>
            (
                new FileUploadingStatusDto()
                {
                    Id = fileId,
                    UploadedBytes = status.Value,
                    Code = file!.Code!,
                    Status = file.Status!.Value
                }
            );
        }

        public async Task<ServiceResult<FileObjDto>> UpdateFileObjectNameAsync(UpdateFileObjectNameDto updateFileObjectNameDto, long userId)
        {
            throw new NotImplementedException();
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
            fileObj.UploadFinishTime = DateTime.UtcNow;
            fileObj.LastUpdateTime = DateTime.UtcNow;
            fileObj.Status = FileStatus.Wait;
            fileObj.TempFileLocation = tempFileName;

            await _fileBasicRepository.InsertAsync(fileObj);
            await _redisProvider.HSetAsync($"{Const.REDIS_UPLOADFILE_CACHE_KEY}{userId}", fileObj.Id.ToString(), (long)0, TimeSpan.FromDays(3));

            return new ServiceResult<UploadFileResponseDto>(new UploadFileResponseDto()
            {
                FileId = fileObj.Id
            });
        }
    }
}
