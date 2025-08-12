
using FluentValidation;
using Warehouse.Services.Iservices;

namespace Warehouse.Services.DTO.Validators
{
    public class ClientDtoValidator : AbstractValidator<ClientDto>
    {
        
        public ClientDtoValidator(IClientService ClientService)
        {
            RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Имя обязательно для заполнения")
            .MaximumLength(100);

            RuleFor(x => x).Must( dto =>  !ClientService.IsNameExist(dto.Id, dto.Name))
            .WithMessage("Клиент с таким именем уже существует");

            RuleFor(x => x.Status)
                .IsInEnum().WithMessage("Недопустимый статус");

            RuleFor(x => x.Id)
                .Must(id => ClientService.IsIdExist(id))
                .WithMessage("Клиент с таким идентификатором не существует");
                

        }
    }
}
