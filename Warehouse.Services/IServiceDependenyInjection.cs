
using Microsoft.Extensions.DependencyInjection;

using Warehouse.Domain;
using Warehouse.Data;
using Warehouse.Services;
using Warehouse.Services.services;
using Warehouse.Services.Iservices;
using FluentValidation;
using Warehouse.Services.DTO.Validators;
using Warehouse.Services.DTO;



namespace Services.DependenyInjection
{
    public static class IServiceDependenyInjection
    {
        public static void SetDependencies(this IServiceCollection serviceCollection/*, IConfigurationRoot configuration*/)
        {
            #region important region

            serviceCollection.AddScoped<IUnitOfWork, UnitOfWork>();
            serviceCollection.AddAutoMapper(typeof(MappingProfiles));

            #endregion



            serviceCollection.AddScoped<IResourceService, ResourceService>();
            serviceCollection.AddScoped<IUnitService, UnitService>();
            serviceCollection.AddScoped<IClientService, ClientService>();
            serviceCollection.AddScoped<IReceiptService, ReceiptService>();
            serviceCollection.AddScoped<IShipmentsService, ShipmentsService>();
            serviceCollection.AddScoped<IBalanceService, BalanceService>();


            serviceCollection.AddTransient<IValidator<ResourceDto>, ResourceDtoValidator>();
            serviceCollection.AddTransient<IValidator<ClientDto>, ClientDtoValidator>();
            serviceCollection.AddTransient<IValidator<InboundItemDto>, InboundItemDtoValidator>();
            serviceCollection.AddTransient<IValidator<OutboundItemDto>, OutboundItemDtoValidator>();
            serviceCollection.AddTransient<IValidator<ReceiptDto>, ReceiptDtoValidator>();
            serviceCollection.AddTransient<IValidator<ShipmentDto>, ShipmentDtoValidator>();
            serviceCollection.AddTransient<IValidator<UnitDto>, UnitDtoValidator>();


        }


    }
}
