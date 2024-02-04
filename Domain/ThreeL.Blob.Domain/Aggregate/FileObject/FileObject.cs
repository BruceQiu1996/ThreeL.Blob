using ThreeL.Blob.Infra.Repository.Entities;
using ThreeL.Blob.Shared.Domain.Entities;
using ThreeL.Blob.Shared.Domain.Metadata.FileObject;

namespace ThreeL.Blob.Domain.Aggregate.FileObject
{
    public class FileObject : AggregateRoot<long>, ISoftDelete, IBasicAuditInfo<long>
    {
        public string Name { get; set; }
        public string? Location { get; set; }
        public string? Code { get; set; }
        public long? Size { get; set; }
        public long? ParentFolder { get; set; }
        public bool IsDeleted { get; set; }
        public long CreateBy { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime UploadFinishTime { get; set; }
        public DateTime LastUpdateTime { get; set; }
        public bool IsFolder { get; set; }
        public FileStatus? Status { get; set; }
        public string? TempFileLocation { get; set; }
        public string? ThumbnailImageLocation { get; set; }

        public FileObject() { }

        //创建压缩文件
        public FileObject(string name, string location, string code, long parentFolder, long createBy, DateTime createTime, long size)
        {
            Name = name;
            Location = location;
            Code = code;
            ParentFolder = parentFolder;
            CreateBy = createBy;
            CreateTime = createTime;
            UploadFinishTime = createTime;
            LastUpdateTime = createTime;
            IsFolder = false;
            Status = FileStatus.Normal;
            Size = size;
        }
    }
}
