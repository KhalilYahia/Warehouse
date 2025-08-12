using AutoMapper;
using AutoMapper.Features;
using Warehouse.Common;
using Warehouse.Domain.Entities;
using Warehouse.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Warehouse.Services.DTO;

namespace Warehouse.Services
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            #region Dto to Entity
            CreateMap<ResourceDto, Resource>()
                 .ForMember(dest => dest.Id, opt => opt.Ignore()); // prevent ID overwrite
            CreateMap<UnitDto, Unit>()
                 .ForMember(dest => dest.Id, opt => opt.Ignore()); // prevent ID overwrite
            CreateMap<ClientDto, Client>()
                .ForMember(dest => dest.Id, opt => opt.Ignore()); // prevent ID overwrite

            CreateMap<ReceiptDto, InboundDocument>()
                 .ForMember(dest => dest.Id, opt => opt.Ignore()); // prevent ID overwrite
            CreateMap<InboundItemDto, InboundItem>()
                 .ForMember(dest => dest.Id, opt => opt.Ignore()); // prevent ID overwrite

            CreateMap<ShipmentDto, OutboundDocument>()
                .ForMember(dest => dest.Id, opt => opt.Ignore()); // prevent ID overwrite
            CreateMap<OutboundItemDto, OutboundItem>()
                .ForMember(dest => dest.Id, opt => opt.Ignore()); // prevent ID overwrite


            #endregion

            //



            #region Entity To Dto 
            CreateMap<Resource, ResourceDto>();
            CreateMap<Unit, UnitDto>();
            CreateMap<Client, ClientDto>();
            CreateMap<InboundDocument, InboundDocDto>();
            //

           
            #endregion

        }

    }
}
