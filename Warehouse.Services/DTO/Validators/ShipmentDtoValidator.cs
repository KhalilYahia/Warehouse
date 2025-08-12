using Castle.Core.Resource;
using FluentValidation;
using Warehouse.Common;
using Warehouse.Services.Iservices;
using Warehouse.Services.services;

namespace Warehouse.Services.DTO.Validators
{
    public class ShipmentDtoValidator: AbstractValidator<ShipmentDto>
    {
        
        public ShipmentDtoValidator(IValidator<OutboundItemDto> itemValidator, IShipmentsService _IShipmentsService)
        {
            RuleFor(x => x.Id)
                .Must(Id => _IShipmentsService.IsIdExist(Id))
                .WithMessage("Отгрузочный документ с таким идентификатором не существует");


            RuleFor(x => x.Number)
                 .NotEmpty().WithMessage("Номер обязателен для заполнения")
                 .MaximumLength(25).WithMessage("Номер должен содержать не более 25 символов");

            RuleFor(x => x.Date)
                .NotEmpty().WithMessage("Дата обязательна для заполнения")
                .LessThanOrEqualTo(Utils.ServerNow).WithMessage("Дата не может быть в будущем");

            RuleFor(x => x.ClientId).Must(ClientId => _IShipmentsService.IsClientExistAndActive(ClientId))
                .WithMessage("Такой клиент не существует или не активен ");

            RuleFor(x => x).Must(dto => !_IShipmentsService.IsSigned(dto.Id))
                .WithMessage("Документ подписан, обновление невозможно. Сначала снимите подпись");

            RuleForEach(x => x.Goods)
                .SetValidator(itemValidator);
        }
    }
}
