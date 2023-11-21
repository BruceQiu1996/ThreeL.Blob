using AutoMapper;
using ThreeL.Blob.Clients.Win.Dtos;
using ThreeL.Blob.Clients.Win.ViewModels.Apply;
using ThreeL.Blob.Clients.Win.ViewModels.Item;

namespace ThreeL.Blob.Clients.Win.Profiles
{
    public class RelationProfile : Profile
    {
        public RelationProfile()
        {
            CreateMap<RelationBriefDto, RelationItemViewModel>();
            CreateMap<RelationBriefDto, UnRelationItemViewModel>();
            CreateMap<ApplyDto, AddFriendApplyMessageViewModel>().
                ForMember(x => x.CreateDate, y =>
                {
                    y.MapFrom(x => x.CreateTime.ToString("HH:mm:ss"));
                });
        }
    }
}
