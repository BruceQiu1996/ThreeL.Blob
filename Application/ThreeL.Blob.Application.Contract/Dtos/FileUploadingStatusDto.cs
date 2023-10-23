﻿using ThreeL.Blob.Shared.Domain.Metadata.FileObject;

namespace ThreeL.Blob.Application.Contract.Dtos
{
    public class FileUploadingStatusDto
    {
        public long Id { get; set; }
        public long UploadedBytes { get; set; }
        public string Code { get; set; }
        public FileStatus Status { get; set; }
    }
}
