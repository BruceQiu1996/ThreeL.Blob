using AutoMapper;
using ThreeL.Blob.Application.Contract.Dtos;
using ThreeL.Blob.Domain.Aggregate.User;

namespace ThreeL.Blob.Application.Contract.Profiles
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<User, UserLoginResponseDto>()
                .ForMember(x => x.Role, y =>
                {
                    y.MapFrom(src => src.Role.ToString());
                });

            CreateMap<User, RelationBriefDto>().AfterMap((x, y) =>
            {
                y.IsGroup = false;
            });

            CreateMap<FriendApply, ApplyDto>();
        }
    }
}
