using AutoMapper;
using ThreeL.Blob.Clients.Win.ViewModels.Item;

namespace ThreeL.Blob.Clients.Win.Profiles
{
    internal class TransferProfile : Profile
    {
        public TransferProfile()
        {
            CreateMap<Entities.UploadFileRecord, UploadItemViewModel>();
        }
    }
}
