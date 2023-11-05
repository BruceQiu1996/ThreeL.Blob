using AutoMapper;
using ThreeL.Blob.Application.Contract.Dtos;
using ThreeL.Blob.Domain.Aggregate.FileObject;

namespace ThreeL.Blob.Application.Contract.Profiles
{
    public class FileObjectProfile : Profile
    {
        public FileObjectProfile()
        {
            CreateMap<UploadFileDto, FileObject>();
            CreateMap<FileObject, FileObjDto>();
            CreateMap<FileObject, PreDownloadFolderFileItemResponseDto>();
        }
    }
}
