using Castle.Core.Resource;
using FluentValidation;
using Warehouse.Services.Iservices;
using Warehouse.Services.services;

namespace Warehouse.Services.DTO.Validators
{
    public class UnitDtoValidator : AbstractValidator<UnitDto>
    {
        
        public UnitDtoValidator(IUnitService UnitService)
        {
            RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Имя обязательно для заполнения")
            .MaximumLength(100);

            RuleFor(x => x).Must(dto => !UnitService.IsNameExist(dto.Id, dto.Name))
            .WithMessage("Единица с таким именем уже существует");
            

            RuleFor(x => x.Status)
                .IsInEnum().WithMessage("Недопустимый статус");

            RuleFor(x => x.Id)
                .Must(Id => UnitService.IsIdExist(Id))
                .WithMessage("Такая единица не существует");
                

        }
    }
}
