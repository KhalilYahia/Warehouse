using Castle.Core.Resource;
using FluentValidation;
using Warehouse.Services.Iservices;
using Warehouse.Services.services;

namespace Warehouse.Services.DTO.Validators
{
    public class OutboundItemDtoValidator : AbstractValidator<OutboundItemDto>
    {
        
        public OutboundItemDtoValidator(IShipmentsService _IShipmentsService)
        {
            RuleFor(x => x.Quantity)
                .NotEmpty()
                .GreaterThan(0)                
                .WithMessage("Количество должно быть больше 0");

            RuleFor(x => x.UnitId).Must(UnitId => _IShipmentsService.IsUnitExistAndActive(UnitId))
                .WithMessage("Такая единица не существует или неактивна");

           

            RuleFor(x => x.ResourceId).Must(ResourceId => _IShipmentsService.IsResourceExistAndActive(ResourceId))
                .WithMessage("Такой ресурс не существует или неактивен");
    
  

        }
    }
}
