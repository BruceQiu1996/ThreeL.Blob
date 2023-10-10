using FluentValidation;
using ThreeL.Blob.Application.Contract.Dtos;

namespace ThreeL.Blob.Application.Contract.Validators.User
{
    public class UserCreationDtoValidator : AbstractValidator<UserCreationDto>
    {
        public UserCreationDtoValidator()
        {
            RuleFor(x => x.UserName).NotNull().NotEmpty().MinimumLength(2)
                .MaximumLength(16).WithName("用户名");
            RuleFor(x => x.Password).NotNull().NotEmpty().MinimumLength(6)
                .MaximumLength(16).WithName("密码");
        }
    }
}
