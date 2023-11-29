using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Net;
using ThreeL.Blob.Application.Channels;
using ThreeL.Blob.Application.Contract.Dtos;
using ThreeL.Blob.Application.Contract.Services;
using ThreeL.Blob.Domain.Aggregate.FileObject;
using ThreeL.Blob.Domain.Aggregate.User;
using ThreeL.Blob.Infra.Redis;
using ThreeL.Blob.Infra.Repository.IRepositories;
using ThreeL.Blob.Shared.Application.Contract.Configurations;
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
        private readonly DeleteFilesChannel _deleteFilesChannel;
        public FileService(IEfBasicRepository<User, long> userBasicRepository,
                           IEfBasicRepository<FileObject, long> fileBasicRepository,
                           IEfBasicRepository<DownloadFileTask, string> downloadTaskBasicRepository,
                           IRedisProvider redisProvider,
                           IMapper mapper,
                           IConfiguration configuration,
                           DeleteFilesChannel deleteFilesChannel)
        {
            _mapper = mapper;
            _redisProvider = redisProvider;
            _downloadTaskBasicRepository = downloadTaskBasicRepository;
            _userBasicRepository = userBasicRepository;
            _fileBasicRepository = fileBasicRepository;
            _configuration = configuration;
            _deleteFilesChannel = deleteFilesChannel;
        }

        public async Task<ServiceResult<PreDownloadFolderResponseDto>> PreDownloadFolderAsync(long folderId)
        {
            List<FileObject> fileObjects = new List<FileObject>();
            var root = await _fileBasicRepository.GetAsync(folderId);
            if (root == null)
            {
                return new ServiceResult<PreDownloadFolderResponseDto>(HttpStatusCode.BadRequest, "文件数据异常");
            }

            var files = await _fileBasicRepository
                   .QuerySqlAsync($"WITH RECURSIVE file_teee As(SELECT * FROM fileobject WHERE fileobject.Id = {root.Id} UNION SELECT f1.* FROM fileobject f1 JOIN file_teee ON f1.ParentFolder = file_teee.id) SELECT * FROM file_teee WHERE file_teee.Status = 4");

            var resp = new PreDownloadFolderResponseDto()
            {
                Size = files.Sum(x => x.Size ?? 0),
                Items = files.Select(_mapper.Map<PreDownloadFolderFileItemResponseDto>)
            };

            return new ServiceResult<PreDownloadFolderResponseDto>(resp);
        }

        public async Task<ServiceResult> CancelDownloadingAsync(string taskId, long userId)
        {
            var task = await _downloadTaskBasicRepository.GetAsync(taskId);
            if (task == null || task.Status != DownloadTaskStatus.Downloading || task.Status != DownloadTaskStatus.DownloadingSuspend
                || task.Status != DownloadTaskStatus.Wait)
            {
                return new ServiceResult();
            }

            task.Status = DownloadTaskStatus.Cancelled;
            await _downloadTaskBasicRepository.UpdateAsync(task);

            return new ServiceResult();
        }

        public async Task<ServiceResult<FileUploadingStatusDto>> CancelUploadingAsync(long fileId, long userId)
        {
            var file = await _fileBasicRepository.FirstOrDefaultAsync(x => x.Id == fileId && x.CreateBy == userId);
            if (file == null)
                return new ServiceResult<FileUploadingStatusDto>(new FileUploadingStatusDto()
                {
                    Id = fileId,
                    Status = FileStatus.Cancel,
                });

            file.Status = FileStatus.Cancel;
            file.UploadFinishTime = DateTime.Now;
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
            var exist = await _fileBasicRepository
                .FirstOrDefaultAsync(x => x.IsFolder && x.Name == folderCreationDto.FolderName && x.ParentFolder == folderCreationDto.ParentId && x.CreateBy == userId);
            if (exist != null)
            {
                folderCreationDto.FolderName = $"{folderCreationDto.FolderName}_{DateTime.Now.ToString("yyyyMMddhhmmssfff")}";
            }
            var fileObject = new FileObject()
            {
                CreateBy = userId,
                CreateTime = DateTime.Now,
                LastUpdateTime = DateTime.Now,
                UploadFinishTime = DateTime.Now,
                IsFolder = true,
                Name = folderCreationDto.FolderName,
                ParentFolder = folderCreationDto.ParentId,
                Location = folderLocation,
                Status = FileStatus.Normal
            };
            await _fileBasicRepository.InsertAsync(fileObject);

            return new ServiceResult<FileObjDto>(_mapper.Map<FileObjDto>(fileObject));
        }

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

            await _fileBasicRepository.RemoveRangeAsync(fileObjects);
            await _deleteFilesChannel.WriteMessageAsync(fileObjects);

            return new ServiceResult();
        }

        public async Task<ServiceResult<DownloadFileResponseDto>> DownloadAsync(long fileId, long userId)
        {
            var file = await _fileBasicRepository.FirstOrDefaultAsync(x => x.Id == fileId && x.CreateBy == userId);
            if (file == null || file.IsFolder || !File.Exists(file.Location) || file.Status != FileStatus.Normal)
                return new ServiceResult<DownloadFileResponseDto>(HttpStatusCode.BadRequest, "数据异常");

            var task = new DownloadFileTask()
            {
                CreateTime = DateTime.Now,
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
            List<FileObject> items = await _fileBasicRepository
                    .Where(x => x.ParentFolder == parentId && x.CreateBy == userId && x.Status == FileStatus.Normal)
                    .OrderByDescending(x => x.CreateTime).ToListAsync();

            return new ServiceResult<IEnumerable<FileObjDto>>(items.Select(x =>
            {
                x.ThumbnailImageLocation = (string.IsNullOrEmpty(x.ThumbnailImageLocation) || !File.Exists(x.ThumbnailImageLocation)) ? null :
                    Path.GetFileName(x.ThumbnailImageLocation);

                return _mapper.Map<FileObjDto>(x);
            }));
        }

        public async Task<ServiceResult<IEnumerable<FolderSimpleDto>>> GetAllFoldersAsync(long userId)
        {
            var files = await _fileBasicRepository
                        .QuerySqlAsync($"WITH RECURSIVE file_teee As(SELECT * FROM fileobject WHERE fileobject.ParentFolder = 0 AND fileobject.CREATEBY = {userId} AND fileobject.IsFolder = 1 UNION SELECT f1.* FROM fileobject f1 JOIN file_teee ON f1.ParentFolder = file_teee.id AND f1.IsFolder = 1 AND f1.CREATEBY = {userId}) SELECT * FROM file_teee");

            return new ServiceResult<IEnumerable<FolderSimpleDto>>(files.Select(x => new FolderSimpleDto()
            {
                Id = x.Id,
                Name = x.Name,
                ParentId = x.ParentFolder == null ? 0 : x.ParentFolder.Value
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

        public async Task<ServiceResult> UpdateFileObjectNameAsync(UpdateFileObjectNameDto updateFileObjectNameDto, long userId)
        {
            var file = await _fileBasicRepository.FirstOrDefaultAsync(x => x.Id == updateFileObjectNameDto.FileId && x.CreateBy == userId);
            if (file == null)
                return new ServiceResult(HttpStatusCode.BadRequest, "数据异常");

            if (file.Name == updateFileObjectNameDto.Name)
                return new ServiceResult();

            var exist = await _fileBasicRepository.FirstOrDefaultAsync(x => x.ParentFolder == file.ParentFolder && x.Name == updateFileObjectNameDto.Name);
            if (exist != null)
                return new ServiceResult(HttpStatusCode.BadRequest, "文件名已存在");

            file.Name = updateFileObjectNameDto.Name;
            file.LastUpdateTime = DateTime.Now;
            await _fileBasicRepository.UpdateAsync(file);

            return new ServiceResult();
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
            fileObj.CreateTime = DateTime.Now;
            fileObj.UploadFinishTime = DateTime.Now;
            fileObj.LastUpdateTime = DateTime.Now;
            fileObj.Status = FileStatus.Wait;
            fileObj.TempFileLocation = tempFileName;

            await _fileBasicRepository.InsertAsync(fileObj);
            await _redisProvider.HSetAsync($"{Const.REDIS_UPLOADFILE_CACHE_KEY}{userId}", fileObj.Id.ToString(), (long)0, TimeSpan.FromDays(3));

            return new ServiceResult<UploadFileResponseDto>(new UploadFileResponseDto()
            {
                FileId = fileObj.Id
            });
        }

        public async Task<ServiceResult> UpdateFileObjectsLocationAsync(UpdateFileObjectLocationDto updateFileObjectLocationDto, long userId)
        {
            List<FileObject> fileObjects = new List<FileObject>();
            var roots = await _fileBasicRepository.Where(x => updateFileObjectLocationDto.FileIds.Contains(x.Id) && x.CreateBy == userId).ToListAsync();
            if (roots == null || roots.Count <= 0)
            {
                return new ServiceResult(HttpStatusCode.BadRequest, "数据异常");
            }

            long parentId = 0;
            if (updateFileObjectLocationDto.ParentFolder != 0)
            {
                var parent = await _fileBasicRepository.FirstOrDefaultAsync(x => x.Id == updateFileObjectLocationDto.ParentFolder && x.CreateBy == userId);
                if (parent == null)
                {
                    return new ServiceResult(HttpStatusCode.BadRequest, "目标目录异常");
                }

                parentId = parent.Id;
            }

            //目标目录下存在同名文件
            var oldfiles = await _fileBasicRepository
            .Where(x => x.ParentFolder == parentId).ToListAsync();

            if (oldfiles.Any(x => roots.FirstOrDefault(y => y.Name == x.Name) != null))
            {
                return new ServiceResult(HttpStatusCode.BadRequest, $"目标目录下存在同名文件");
            }

            if (roots.Any(x => x.IsFolder)) //移动的存在目录，需要考虑子目录
            {
                var folders = new List<FileObject>();
                foreach (var root in roots.Where(x => x.IsFolder))
                {
                    var files = await _fileBasicRepository
                        .QuerySqlAsync($"WITH RECURSIVE file_teee As(SELECT * FROM fileobject WHERE fileobject.Id = {root.Id} UNION SELECT f1.* FROM fileobject f1 JOIN file_teee ON f1.ParentFolder = file_teee.id AND f1.IsFolder = 1) SELECT * FROM file_teee");

                    folders.AddRange(files);
                }

                if (folders.Any(x => x.Id == updateFileObjectLocationDto.ParentFolder))
                {
                    return new ServiceResult(HttpStatusCode.BadRequest, "目标目录不可以是子目录");
                }
            }

            roots.ForEach(x =>
            {
                x.ParentFolder = parentId;
                x.LastUpdateTime = DateTime.Now;
            });
            await _fileBasicRepository.UpdateRangeAsync(roots);

            return new ServiceResult();
        }

        public async Task<ServiceResult<IEnumerable<FolderTreeCreationResponseDto>>> CreateFoldersAsync(FolderTreeCreationDto folderTreeCreationDto, long userId)
        {
            string parentFolderLocation = null;
            if (folderTreeCreationDto.ParentId != 0)
            {
                var parentFolder = await _fileBasicRepository.GetAsync(folderTreeCreationDto.ParentId);
                if (parentFolder == null || !parentFolder.IsFolder || !Directory.Exists(parentFolder.Location))
                {
                    return new ServiceResult<IEnumerable<FolderTreeCreationResponseDto>>(HttpStatusCode.BadRequest, "目录数据异常");
                }
                parentFolderLocation = parentFolder.Location;
            }
            else
            {
                var user = await _userBasicRepository.GetAsync(userId);
                if (user == null || !Directory.Exists(user.Location))
                {
                    return new ServiceResult<IEnumerable<FolderTreeCreationResponseDto>>(HttpStatusCode.BadRequest, "目录数据异常");
                }
                parentFolderLocation = user.Location;
            }
            folderTreeCreationDto.Items.First().ParentId = folderTreeCreationDto.ParentId;
            folderTreeCreationDto.Items.First().ParentFolderLocation = parentFolderLocation;
            await LoopCreateFoldersAsync(folderTreeCreationDto.Items, userId);

            return new ServiceResult<IEnumerable<FolderTreeCreationResponseDto>>(folderTreeCreationDto.Items.Select(_mapper.Map<FolderTreeCreationResponseDto>));
        }

        private async Task LoopCreateFoldersAsync(IEnumerable<FolderTreeCreationItemDto> creationDtos, long userId)
        {
            foreach (var item in creationDtos)
            {
                var folderName = Guid.NewGuid().ToString();
                var folderLocation = Path.Combine(item.ParentFolderLocation, folderName);
                Directory.CreateDirectory(folderLocation);
                var exist = await _fileBasicRepository
                    .FirstOrDefaultAsync(x => x.IsFolder && x.Name == item.FolderName && x.ParentFolder == item.ParentId && x.CreateBy == userId);
                if (exist != null)
                {
                    item.FolderName = $"{item.FolderName}_{DateTime.Now.ToString("yyyyMMddhhmmssfff")}";
                }

                var fileObject = new FileObject()
                {
                    CreateBy = userId,
                    CreateTime = DateTime.Now,
                    LastUpdateTime = DateTime.Now,
                    UploadFinishTime = DateTime.Now,
                    IsFolder = true,
                    Name = item.FolderName,
                    ParentFolder = item.ParentId,
                    Location = folderLocation,
                    Status = FileStatus.Normal
                };

                await _fileBasicRepository.InsertAsync(fileObject);
                item.RemoteId = fileObject.Id;
                foreach (var innerItem in creationDtos.Where(x => x.ParentClientId == item.ClientId))
                {
                    innerItem.ParentId = fileObject.Id;
                    innerItem.ParentFolderLocation = fileObject.Location;
                }
            }
        }
    }
}
