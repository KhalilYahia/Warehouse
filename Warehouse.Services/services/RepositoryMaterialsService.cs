using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Warehouse.Domain;
using Warehouse.Domain.Entities;
using Warehouse.Services.DTO;
using Warehouse.Services.Iservices;

namespace Warehouse.Services.services
{
    public class RepositoryMaterialsService : IRepositoryMaterialsService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public RepositoryMaterialsService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }


        public async Task<int> AddAsync(RepositoryMaterialsDto materialDto)
        {
            var material = _mapper.Map<RepositoryMaterials>(materialDto);
            material.Repositories = new List<Repository>
            {
                new Repository()
                {
                    CategoryId = materialDto.CategoryId,
                    Name = materialDto.Name,
                    Amount_Out = 0,
                    Amount_In = 0,
                    Amount_Remender = 0,
                    Sort = materialDto.Sort

                }
            };
            _unitOfWork.repository<RepositoryMaterials>().Add(material);
            await _unitOfWork.Complete();
            return material.Id;
        }

        public async Task<bool> UpdateAsync(RepositoryMaterialsDto materialDto)
        {
            var material = _mapper.Map<RepositoryMaterials>(materialDto);
            _unitOfWork.repository<RepositoryMaterials>().UpdateAsync(material);
            await _unitOfWork.Complete();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var material = await _unitOfWork.repository<RepositoryMaterials>().GetByIdAsync(id);
            if (material == null)
            {
                throw new KeyNotFoundException("Material not found");
            }

            _unitOfWork.repository<RepositoryMaterials>().DeleteAsync(material);
            await _unitOfWork.Complete();
            return true;
        }


        public async Task<List<RepositoryMaterialsDto>> GetAllAsync()
        {
            var materials = await _unitOfWork.repository<RepositoryMaterials>().GetAllAsync();
            return _mapper.Map<List<RepositoryMaterialsDto>>(materials.ToList());
        }
        public async Task<List<RepositoryMaterialsDto>> GetAllByCategoryId(int catId)
        {
            var materials = await _unitOfWork.repository<RepositoryMaterials>().Get(m=>m.CategoryId==catId);
            return _mapper.Map<List<RepositoryMaterialsDto>>(materials.ToList());
        }
        
        public async Task<RepositoryMaterialsDto> GetByIdAsync(int id)
        {
            var material = await _unitOfWork.repository<RepositoryMaterials>().GetByIdAsync(id);
            return _mapper.Map<RepositoryMaterialsDto>(material);
        }

      
    }

}
