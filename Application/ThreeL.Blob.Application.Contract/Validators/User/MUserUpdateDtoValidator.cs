using FluentValidation;
using ThreeL.Blob.Application.Contract.Dtos.Management;
using ThreeL.Blob.Infra.Core.Extensions.System;

namespace ThreeL.Blob.Application.Contract.Validators.User
{
    public class MUserUpdateDtoValidator : AbstractValidator<MUserUpdateDto>
    {
        public MUserUpdateDtoValidator()
        {
            RuleFor(x => x.UserName).NotNull().NotEmpty().MinimumLength(5)
                .MaximumLength(16).WithName("用户名").Must(x => x.ValidUserName()).WithMessage("用户名长度5-16，只能由字母或者数字组成"); ;
            RuleFor(x => x.Role).NotNull().WithName("角色").WithMessage("角色不能为空");
            RuleFor(x => x.Size).GreaterThan(0).WithMessage("空间大小不能小于0");
        }
    }
}
