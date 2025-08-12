using Castle.Core.Resource;
using FluentValidation;
using Warehouse.Services.Iservices;
using Warehouse.Services.services;

namespace Warehouse.Services.DTO.Validators
{
    public class InboundItemDtoValidator : AbstractValidator<InboundItemDto>
    {
        
        public InboundItemDtoValidator(IReceiptService _IReceiptService)
        {
            RuleFor(x => x.Quantity)
                .GreaterThanOrEqualTo(0)                
                .WithMessage("Количество должно быть больше или равно 0");

            RuleFor(x => x.UnitId).Must(UnitId =>  _IReceiptService.IsUnitssExistAndActive(UnitId))
                .WithMessage("Такая единица не существует или неактивна");


            RuleFor(x => x.ResourceId).Must(ResourceId => _IReceiptService.IsResourcesExistAndActive(ResourceId))
                .WithMessage("Такой ресурс не существует или неактивен");
    
  

        }
    }
}
