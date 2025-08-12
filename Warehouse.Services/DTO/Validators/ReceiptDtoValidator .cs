using Castle.Core.Resource;
using FluentValidation;
using Warehouse.Common;
using Warehouse.Services.Iservices;
using Warehouse.Services.services;

namespace Warehouse.Services.DTO.Validators
{
    public class ReceiptDtoValidator: AbstractValidator<ReceiptDto>
    {
        
        public ReceiptDtoValidator(IValidator<InboundItemDto> itemValidator, IReceiptService _IReceiptService)
        {
            RuleFor(x => x.Id)
                .Must(Id => _IReceiptService.IsIdExist(Id))
                .WithMessage("Входящий документ с таким идентификатором не существует");


            RuleFor(x => x.Number)
                 .NotEmpty().WithMessage("Номер обязателен для заполнения")
                 .MaximumLength(25).WithMessage("Номер должен содержать не более 25 символов");
            RuleFor(x => x)
                 .Must(dto => !_IReceiptService.IsInboundDocNumberExist(dto.Id,dto.Number))
                           .WithMessage("Номер документа уже существует");

            RuleFor(x => x.Date)
                .NotEmpty().WithMessage("Дата обязательна для заполнения")
                .LessThanOrEqualTo(Utils.ServerNow).WithMessage("Дата не может быть в будущем");

            RuleForEach(x => x.Goods)
                .SetValidator(itemValidator);
        }
    }
}
