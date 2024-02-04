using FluentValidation;
using ThreeL.Blob.Application.Contract.Dtos;

namespace ThreeL.Blob.Application.Contract.Validators.FileObject
{
    public class UploadFileDtoValidator : AbstractValidator<UploadFileDto>
    {
        public UploadFileDtoValidator()
        {
            RuleFor(x => x.Name).NotNull().NotEmpty()
                .Must(x => Path.GetInvalidFileNameChars().All(y => !x.Contains(y))).WithMessage("文件名非法").WithName("文件名");
            RuleFor(x => x.ParentFolder).GreaterThanOrEqualTo(0).WithName("父文件夹");
            RuleFor(x => x.Code).NotNull().NotEmpty().WithName("文件校验码");
        }
    }
}
