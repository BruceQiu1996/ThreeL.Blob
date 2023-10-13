using AutoMapper;
using ThreeL.Blob.Clients.Win.Dtos;

namespace ThreeL.Blob.Clients.Win.Profiles
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<UserLoginResponseDto, Entities.UserProfile>();
        }
    }
}
