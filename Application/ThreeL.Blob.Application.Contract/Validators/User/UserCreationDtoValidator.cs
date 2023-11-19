using FluentValidation;
using ThreeL.Blob.Application.Contract.Dtos;
using ThreeL.Blob.Infra.Core.Extensions.System;

namespace ThreeL.Blob.Application.Contract.Validators.User
{
    public class UserCreationDtoValidator : AbstractValidator<UserCreationDto>
    {
        public UserCreationDtoValidator()
        {
            RuleFor(x => x.UserName).NotNull().NotEmpty().MinimumLength(5)
                .MaximumLength(16).WithName("用户名").Must(x => x.ValidUserName()).WithMessage("用户名长度5-16，只能由字母或者数字组成"); ;
            RuleFor(x => x.Password).NotNull().NotEmpty().MinimumLength(6)
                .MaximumLength(16).WithName("密码").Must(x => x.ValidPassword()).WithMessage("密码长度6-16，并且需要包含字母和数字");
        }
    }
}
