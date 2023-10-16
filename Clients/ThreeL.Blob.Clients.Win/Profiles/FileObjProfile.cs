using AutoMapper;
using ThreeL.Blob.Clients.Win.Dtos;
using ThreeL.Blob.Clients.Win.ViewModels.Item;

namespace ThreeL.Blob.Clients.Win.Profiles
{
    public class FileObjProfile : Profile
    {
        public FileObjProfile()
        {
            CreateMap<FileObjDto, FileObjItemViewModel>();
        }
    }
}
