using FluentValidation;
using ThreeL.Blob.Application.Contract.Dtos;

namespace ThreeL.Blob.Application.Contract.Validators.FileObject
{
    public class FolderCreationDtoValidator : AbstractValidator<FolderCreationDto>
    {
        public FolderCreationDtoValidator()
        {
            RuleFor(x => x.FolderName)
                .NotEmpty().NotNull().Must(x => Path.GetInvalidPathChars().All(y => !x.Contains(y))).WithMessage("目录名非法").WithName("目录名");
        }
    }
}
