using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
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
        private readonly IEfBasicRepository<FileObjectShareRecord, long> _fileObjectShareRecordEfBasicRepository;
        private readonly IRedisProvider _redisProvider;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly DeleteFilesChannel _deleteFilesChannel;
        private readonly CompressFileObjectsChannel _compressFileObjectsChannel;
        private readonly ILogger<FileService> _logger;
        public FileService(IEfBasicRepository<User, long> userBasicRepository,
                           IEfBasicRepository<FileObject, long> fileBasicRepository,
                           IEfBasicRepository<DownloadFileTask, string> downloadTaskBasicRepository,
                           IEfBasicRepository<FileObjectShareRecord, long> fileObjectShareRecordEfBasicRepository,
                           IRedisProvider redisProvider,
                           IMapper mapper,
                           IConfiguration configuration,
                           DeleteFilesChannel deleteFilesChannel,
                           CompressFileObjectsChannel compressFileObjectsChannel,
                           ILogger<FileService> logger)
        {
            _logger = logger;
            _mapper = mapper;
            _redisProvider = redisProvider;
            _downloadTaskBasicRepository = downloadTaskBasicRepository;
            _fileObjectShareRecordEfBasicRepository = fileObjectShareRecordEfBasicRepository;
            _userBasicRepository = userBasicRepository;
            _fileBasicRepository = fileBasicRepository;
            _configuration = configuration;
            _deleteFilesChannel = deleteFilesChannel;
            _compressFileObjectsChannel = compressFileObjectsChannel;
        }

        public async Task<ServiceResult<PreDownloadFolderResponseDto>> PreDownloadFolderAsync(long folderId)
        {
            List<FileObject> FileObjects = new List<FileObject>();
            var root = await _fileBasicRepository.GetAsync(folderId);
            if (root == null)
            {
                return new ServiceResult<PreDownloadFolderResponseDto>(HttpStatusCode.BadRequest, "文件数据异常");
            }

            var files = await _fileBasicRepository
                   .QuerySqlAsync($"WITH RECURSIVE file_teee As(SELECT * FROM FileObject WHERE FileObject.Id = {root.Id} UNION SELECT f1.* FROM FileObject f1 JOIN file_teee ON f1.ParentFolder = file_teee.id) SELECT * FROM file_teee WHERE file_teee.Status = 4");

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
            var parentFolder = await _fileBasicRepository.GetAsync(folderCreationDto.ParentId);
            if (parentFolder == null || !parentFolder.IsFolder || !Directory.Exists(parentFolder.Location))
            {
                return new ServiceResult<FileObjDto>(HttpStatusCode.BadRequest, "目录数据异常");
            }
            var parentFolderLocation = parentFolder.Location;
            var exist = await _fileBasicRepository
                .FirstOrDefaultAsync(x => x.IsFolder && x.Name == folderCreationDto.FolderName && x.ParentFolder == folderCreationDto.ParentId && x.CreateBy == userId);
            if (exist != null)
            {
                folderCreationDto.FolderName = $"{folderCreationDto.FolderName}_{DateTime.Now.ToString("yyyyMMddhhmmssfff")}";
            }

            var folderLocation = Path.Combine(parentFolderLocation, folderCreationDto.FolderName);

            try
            {
                Directory.CreateDirectory(folderLocation);
            }
            catch (ArgumentException ex)
            {
                return new ServiceResult<FileObjDto>(HttpStatusCode.BadRequest, "非法的文件夹名");
            }
            catch (Exception ex)
            {
                return new ServiceResult<FileObjDto>(HttpStatusCode.BadRequest, "服务器异常");
            }

            var FileObject = new FileObject()
            {
                CreateBy = userId,
                CreateTime = DateTime.Now,
                LastUpdateTime = DateTime.Now,
                UploadFinishTime = DateTime.Now,
                IsFolder = true,
                Name = folderCreationDto.FolderName,
                ParentFolder = folderCreationDto.ParentId,
                Location = folderLocation,
                Status = FileStatus.Normal,
                TrackPath = string.IsNullOrEmpty(parentFolder.TrackPath) ? $"{parentFolder.Id}" : $"{parentFolder.TrackPath},{parentFolder.Id}"
            };
            await _fileBasicRepository.InsertAsync(FileObject);

            return new ServiceResult<FileObjDto>(_mapper.Map<FileObjDto>(FileObject));
        }

        public async Task<ServiceResult> DeleteItemsAsync(long[] fileIds, long userId)
        {
            List<FileObject> FileObjects = new List<FileObject>();
            var roots = await _fileBasicRepository.Where(x => fileIds.Contains(x.Id) && x.CreateBy == userId).ToListAsync();
            if (roots == null || roots.Count <= 0)
            {
                return new ServiceResult();
            }


            FileObjects.AddRange(roots.Where(x => !x.IsFolder));
            foreach (var root in roots.Where(x => x.IsFolder))
            {
                var files = await _fileBasicRepository
                    .QuerySqlAsync($"WITH RECURSIVE file_teee As(SELECT * FROM FileObject WHERE FileObject.Id = {root.Id} UNION SELECT f1.* FROM FileObject f1 JOIN file_teee ON f1.ParentFolder = file_teee.id) SELECT * FROM file_teee");
                FileObjects.AddRange(files);
            }

            await _fileBasicRepository.RemoveRangeAsync(FileObjects);
            await _deleteFilesChannel.WriteMessageAsync(FileObjects);

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
                        .QuerySqlAsync($"WITH RECURSIVE file_teee As(SELECT * FROM FileObject WHERE FileObject.ParentFolder = 0 AND FileObject.CREATEBY = {userId} AND FileObject.IsFolder = 1 UNION SELECT f1.* FROM FileObject f1 JOIN file_teee ON f1.ParentFolder = file_teee.id AND f1.IsFolder = 1 AND f1.CREATEBY = {userId}) SELECT * FROM file_teee");

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

            var exist = await _fileBasicRepository.FirstOrDefaultAsync(x => x.ParentFolder == file.ParentFolder && x.Name == updateFileObjectNameDto.Name && x.CreateBy == userId);
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

            var folder = await _fileBasicRepository.FirstOrDefaultAsync(x => x.IsFolder && x.Id == uploadFileDto.ParentFolder && x.CreateBy == userId);
            if (folder == null)
                return new ServiceResult<UploadFileResponseDto>(HttpStatusCode.BadRequest, "文件夹不存在");

            var location = folder.Location!;

            if (!Directory.Exists(location))
                return new ServiceResult<UploadFileResponseDto>(HttpStatusCode.BadRequest, "数据异常");

            var tempFileName = Path.Combine(location, $"{Path.GetRandomFileName()}.tmp");
            File.Create(tempFileName).Close();
            var fileObj = _mapper.Map<FileObject>(uploadFileDto);
            var exist = await _fileBasicRepository.FirstOrDefaultAsync(x => x.ParentFolder == fileObj.ParentFolder && x.Name == fileObj.Name && x.CreateBy == userId);
            if (exist != null)
            {
                fileObj.Name = $"{Path.GetFileNameWithoutExtension(fileObj.Name)}_{DateTime.Now.ToString("yyyyMMddhhmmssfff")}{Path.GetExtension(fileObj.Name)}";
            }
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
            List<FileObject> FileObjects = new List<FileObject>();
            var roots = await _fileBasicRepository.Where(x => updateFileObjectLocationDto.FileIds.Contains(x.Id) && x.CreateBy == userId).ToListAsync();
            if (roots == null || roots.Count <= 0)
            {
                return new ServiceResult(HttpStatusCode.BadRequest, "数据异常");
            }

            var parent = await _fileBasicRepository.FirstOrDefaultAsync(x => x.Id == updateFileObjectLocationDto.ParentFolder && x.CreateBy == userId);
            if (parent == null)
            {
                return new ServiceResult(HttpStatusCode.BadRequest, "目标目录异常");
            }

            //目标目录下存在同名文件
            var oldfiles = await _fileBasicRepository
            .Where(x => x.ParentFolder == parent.Id && x.CreateBy == userId).ToListAsync();

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
                        .QuerySqlAsync($"WITH RECURSIVE file_teee As(SELECT * FROM FileObject WHERE FileObject.Id = {root.Id} UNION SELECT f1.* FROM FileObject f1 JOIN file_teee ON f1.ParentFolder = file_teee.id AND f1.IsFolder = 1) SELECT * FROM file_teee");

                    folders.AddRange(files);
                }

                if (folders.Any(x => x.Id == updateFileObjectLocationDto.ParentFolder))
                {
                    return new ServiceResult(HttpStatusCode.BadRequest, "目标目录不可以是子目录");
                }
            }

            roots.ForEach(x =>
            {
                x.ParentFolder = parent.Id;
                x.LastUpdateTime = DateTime.Now;
            });
            await _fileBasicRepository.UpdateRangeAsync(roots);

            return new ServiceResult();
        }

        public async Task<ServiceResult<IEnumerable<FolderTreeCreationResponseDto>>> CreateFoldersAsync(FolderTreeCreationDto folderTreeCreationDto, long userId)
        {
            var parentFolder = await _fileBasicRepository.GetAsync(folderTreeCreationDto.ParentId);
            if (parentFolder == null || !parentFolder.IsFolder || !Directory.Exists(parentFolder.Location))
            {
                return new ServiceResult<IEnumerable<FolderTreeCreationResponseDto>>(HttpStatusCode.BadRequest, "目录数据异常");
            }

            folderTreeCreationDto.Items.First().ParentId = folderTreeCreationDto.ParentId;
            folderTreeCreationDto.Items.First().ParentFolderLocation = parentFolder.Location;
            folderTreeCreationDto.Items.First().TrackPath = string.IsNullOrEmpty(parentFolder.TrackPath) ? $"{parentFolder.Id}" : $"{parentFolder.TrackPath},{parentFolder.Id}";
            await LoopCreateFoldersAsync(folderTreeCreationDto.Items, userId);

            return new ServiceResult<IEnumerable<FolderTreeCreationResponseDto>>(folderTreeCreationDto.Items.Select(_mapper.Map<FolderTreeCreationResponseDto>));
        }

        private async Task LoopCreateFoldersAsync(IEnumerable<FolderTreeCreationItemDto> creationDtos, long userId)
        {
            foreach (var item in creationDtos)
            {
                var exist = await _fileBasicRepository
                    .FirstOrDefaultAsync(x => x.IsFolder && x.Name == item.FolderName && x.ParentFolder == item.ParentId && x.CreateBy == userId);
                if (exist != null)
                {
                    item.FolderName = $"{item.FolderName}_{DateTime.Now.ToString("yyyyMMddhhmmssfff")}";
                }

                try
                {
                    var folderLocation = Path.Combine(item.ParentFolderLocation, item.FolderName);
                    Directory.CreateDirectory(folderLocation);

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
                        Status = FileStatus.Normal,
                        TrackPath = item.TrackPath
                    };

                    await _fileBasicRepository.InsertAsync(fileObject);
                    item.RemoteId = fileObject.Id;
                    foreach (var innerItem in creationDtos.Where(x => x.ParentClientId == item.ClientId))
                    {
                        innerItem.ParentId = fileObject.Id;
                        innerItem.ParentFolderLocation = fileObject.Location;
                        innerItem.TrackPath = $"{fileObject.TrackPath},{fileObject.Id}";
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.ToString());
                    continue;
                }
            }
        }

        public async Task<ServiceResult<DownloadFileResponseDto>> DownloadSharedAsync(string token, long userId)
        {
            var record = await _fileObjectShareRecordEfBasicRepository.FirstOrDefaultAsync(x => x.Token == token);
            if (record == null || record.ExpireTime < DateTime.Now || (record.Target != null && record.Target != userId && record.CreateBy != userId))
            {
                return new ServiceResult<DownloadFileResponseDto>(HttpStatusCode.BadRequest, "文件分享已过期或者分享码错误");
            }

            var file = await _fileBasicRepository.FirstOrDefaultAsync(x => x.Id == record.FileObjectId);
            if (file == null || file.IsFolder || !File.Exists(file.Location) || file.Status != FileStatus.Normal)
                return new ServiceResult<DownloadFileResponseDto>(HttpStatusCode.BadRequest, "文件状态异常或文件已被分享者删除");

            var task = new DownloadFileTask()
            {
                CreateTime = DateTime.Now,
                FileId = file.Id,
                CreateBy = userId,
                Status = DownloadTaskStatus.Wait,
            };

            await _downloadTaskBasicRepository.InsertAsync(task);
            await _redisProvider.HSetAsync($"{Const.REDIS_DOWNLOADTASK_CACHE_KEY}{userId}", task.Id, (long)0, TimeSpan.FromDays(3));

            return new ServiceResult<DownloadFileResponseDto>(new DownloadFileResponseDto()
            {
                FileId = file.Id,
                Code = file.Code,
                TaskId = task.Id,
                Size = file.Size!.Value,
                FileName = file.Name
            });
        }

        public async Task<ServiceResult> CompressFileObjectsAsync(long sender, CompressFileObjectsDto compressFileObjectsDto)
        {
            var files = await _fileBasicRepository
                .Where(x => compressFileObjectsDto.Items.Contains(x.Id) && x.Status == FileStatus.Normal).ToListAsync();
            if (files.GroupBy(x => x.ParentFolder).Count() > 1)
            {
                return new ServiceResult(HttpStatusCode.BadRequest, "只能压缩同一目录下的文件");
            }

            if (files.Count != compressFileObjectsDto.Items.Count())
            {
                return new ServiceResult(HttpStatusCode.BadRequest, "文件状态异常");
            }

            string rootLocaiton = string.Empty;
            var folder = await _fileBasicRepository.GetAsync(files.First().ParentFolder!.Value);
            if (folder == null || !Directory.Exists(folder.Location))
            {
                return new ServiceResult(HttpStatusCode.BadRequest, "目录数据异常");
            }

            await _compressFileObjectsChannel.WriteMessageAsync((compressFileObjectsDto.ZipName, folder, sender, files.ToArray()));

            return new ServiceResult();
        }

        public async Task<ServiceResult<(int, IEnumerable<FileObjectShareRecordDto>)>> GetShareFileRecordsAsyncByPage(long userId, string keyWord, int page)
        {
            var query = _fileObjectShareRecordEfBasicRepository.Where(x => x.CreateBy == userId)
                .LeftJoin(_userBasicRepository.All(), x => x.Target, x => x.Id, (x, y) =>
                new FileObjectShareRecordDto
                {
                    Id = x.Id,
                    Token = x.Token,
                    FileObjectId = x.FileObjectId,
                    CreateTime = x.CreateTime,
                    ExpireTime = x.ExpireTime,
                    TargetName = y.UserName
                }).LeftJoin(_fileBasicRepository.All(), x => x.FileObjectId, x => x.Id, (x, y) =>
                new FileObjectShareRecordDto
                {
                    Id = x.Id,
                    Token = x.Token,
                    FileObjectId = x.FileObjectId,
                    CreateTime = x.CreateTime,
                    ExpireTime = x.ExpireTime,
                    TargetName = x.TargetName,
                    FileObjectName = y.Name,
                    FileSize = y.Size,
                    IsFolder = y.IsFolder
                });

            if (!string.IsNullOrEmpty(keyWord))
            {
                query.Where(x => x.TargetName.Contains(keyWord) || x.FileObjectName.Contains(keyWord));
            }

            var count = query.Count();
            query.Skip((page - 1) * 20).Take(20).OrderByDescending(x => x.CreateTime);

            return new ServiceResult<(int, IEnumerable<FileObjectShareRecordDto>)>((count, await query.ToListAsync()));
        }

        public async Task<ServiceResult<FileObjDto>> GetRootItemAsync(long userId)
        {
            var fileObj = await _fileBasicRepository
                .FirstOrDefaultAsync(x => x.CreateBy == userId && x.IsFolder && x.ParentFolder == null);

            if (fileObj == null)
            {
                return new ServiceResult<FileObjDto>(HttpStatusCode.NotFound, "找不到根目录");
            }

            return new ServiceResult<FileObjDto>(_mapper.Map<FileObjDto>(fileObj));
        }
    }
}