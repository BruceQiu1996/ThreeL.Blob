using ThreeL.Blob.Domain.Aggregate.User;

namespace ThreeL.Blob.Application.Contract.Dtos
{
    public class UserCreationDto
    {
        public string UserName { get; set; }
        public string Password { get; set; }

        public User ToUser(long creator) 
        {
            return new User
            {
                UserName = UserName,
                Role = Shared.Domain.Metadata.User.Role.User,
                CreateTime = DateTime.Now,
                CreateBy = creator,
            };
        }
    }
}
