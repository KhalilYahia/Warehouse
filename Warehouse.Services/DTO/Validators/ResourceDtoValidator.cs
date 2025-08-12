using Castle.Core.Resource;
using FluentValidation;
using Warehouse.Services.Iservices;

namespace Warehouse.Services.DTO.Validators
{
    public class ResourceDtoValidator : AbstractValidator<ResourceDto>
    {
        
        public ResourceDtoValidator(IResourceService resourceService)
        {
            RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Имя обязательно для заполнения")
            .MaximumLength(100);

            RuleFor(x => x).Must(dto =>  !resourceService.IsNameExist(dto.Id, dto.Name))
            .WithMessage("Ресурс с таким именем уже существует");

            RuleFor(x => x.Status)
                .IsInEnum().WithMessage("Недопустимый статус");

            RuleFor(x => x.Id)
                .Must(Id => resourceService.IsIdExist(Id))
                .WithMessage("Такой ресурс не существует");
                

        }
    }
}
