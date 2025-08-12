using YaznGhanem.Domain;
using YaznGhanem.Services.Iservices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using YaznGhanem.Common;
using YaznGhanem.Domain.Entities;
using YaznGhanem.Services.DTO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace YaznGhanem.Services.services
{
    public class PropertyService : IPropertyService
    {

        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public PropertyService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<int> AddNewProperty(PropertyDto dto)
        {
            var model = _mapper.Map<Properties>(dto);
            _unitOfWork.repository<Properties>().Add(model);
            await _unitOfWork.Complete();

            return model.Id;
        }

        public async Task<bool> EditProperty(PropertyDto dto)
        {
            var model = (await _unitOfWork.repository<Properties>().Get(m => m.Id == dto.Id));
            if ((!model.IsNullOrEmpty()) && model.Any())
            {
                var first_model = model.FirstOrDefault();
                PropertyCopier<PropertyDto, Properties>.Copy(dto, first_model);

                _unitOfWork.repository<Properties>().Update(first_model);
                await _unitOfWork.Complete();

                return true;
                
            }
            return false;
        }


        public async Task<bool> RemoveProperty(int PropertyId)
        {
            var model = (await _unitOfWork.repository<Properties>().Get(m => m.Id == PropertyId));
            if((!model.IsNullOrEmpty()) && model.Any())
            {
                var first_model = model.FirstOrDefault();
                if (first_model.AdvertisementProperties.Any())
                    return false;
                else
                {
                    _unitOfWork.repository<Properties>().Delete(first_model);
                    await _unitOfWork.Complete();

                    return true;
                }
            }
            return false;

        }

        public async Task<List<PropertyDto>> GetAllPropertiesForAdmin()
        {
            var models = (await _unitOfWork.repository<Properties>().GetAllAsync()).ToList();
            var res = _mapper.Map<List<Properties>, List<PropertyDto>>(models);

            return res;
        }

        public async Task<List<PropertyDto>> GetAllPropertiesForUser()
        {
            var models = (await _unitOfWork.repository<Properties>().Get(m=>m.IsActive)).ToList();
            var res = _mapper.Map<List<Properties>, List<PropertyDto>>(models);

            return res;
        }


    }
}
