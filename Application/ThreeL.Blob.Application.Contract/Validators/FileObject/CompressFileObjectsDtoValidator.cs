using FluentValidation;
using ThreeL.Blob.Application.Contract.Dtos;

namespace ThreeL.Blob.Application.Contract.Validators.FileObject
{
    public class CompressFileObjectsDtoValidator : AbstractValidator<CompressFileObjectsDto>
    {
        public CompressFileObjectsDtoValidator()
        {
            RuleFor(x => x.ZipName)
                .NotEmpty().NotNull().Must(x => Path.GetInvalidFileNameChars().All(y => !x.Contains(y))).WithMessage("压缩文件名非法").WithName("压缩文件名");
            RuleFor(x => x.Items).NotNull().Must(x => x.Count() > 0).WithName("压缩列表");
        }
    }
}
