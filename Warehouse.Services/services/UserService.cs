using Warehouse.Domain;
using Warehouse.Services.Iservices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Warehouse.Common;
using Warehouse.Domain.Entities;
using Warehouse.Services.DTO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Warehouse.Domain.Entities;
using Castle.Components.DictionaryAdapter.Xml;

namespace Warehouse.Services.services
{
    public class UserService : IUserService
    {

        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public UserService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

     
        public async Task<List<UserDto>> GetAllUsers_forAdmin()
        {
            var models = (await _unitOfWork.repository<CustomUser>().GetAllAsync()).ToList();
            
            var res = _mapper.Map<List<UserDto>>(models);

            return res;
        }

        public async Task<UserDto> GetUserById_forAdmin(string userId)
        {
            var models = (await _unitOfWork.repository<CustomUser>().Get(m=>m.Id== userId)).ToList();
            if(models.Any())
            {
                var model = models.FirstOrDefault();
                var res = _mapper.Map<UserDto>(model);
                return res;
            }

            return new UserDto();
          
        }


        public async Task<List<RoleDto>> GetAllRoles_forAdmin()
        {
            var models = (await _unitOfWork.repository<CustomRole>().GetAllAsync()).ToList();

            var res = _mapper.Map<List<RoleDto>>(models);

            return res;
        }

    }
}
