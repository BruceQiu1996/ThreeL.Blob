using AutoMapper;
using ThreeL.Blob.Clients.Win.Dtos;
using ThreeL.Blob.Clients.Win.ViewModels.Item;

namespace ThreeL.Blob.Clients.Win.Profiles
{
    public class RelationProfile : Profile
    {
        public RelationProfile()
        {
            CreateMap<RelationBriefDto, RelationItemViewModel>();
        }
    }
}
