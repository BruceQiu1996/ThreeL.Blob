using FluentValidation;
using ThreeL.Blob.Application.Contract.Dtos;
using ThreeL.Blob.Infra.Core.Extensions.System;

namespace ThreeL.Blob.Application.Contract.Validators.User
{
    public class UserModifyPasswordDtoValidator : AbstractValidator<UserModifyPasswordDto>
    {
        public UserModifyPasswordDtoValidator()
        {
            RuleFor(x => x.OldPassword).NotNull().NotEmpty().MinimumLength(6)
                .MaximumLength(16).WithName("旧密码").WithMessage("旧密码不正确");
            RuleFor(x => x.NewPassword).NotNull().NotEmpty().MinimumLength(6)
                .MaximumLength(16).WithName("密码").Must(x=>x.ValidPassword()).WithMessage("密码长度6-16，并且需要包含字母和数字");
        }
    }
}
