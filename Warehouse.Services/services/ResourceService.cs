
using AutoMapper;
using Microsoft.AspNetCore.Hosting;
using LLama.Common;
using LLama;
using Warehouse.Common;
using Warehouse.Domain.Entities;
using Warehouse.Services.Iservices;
using Warehouse.Domain;
using Warehouse.Services.DTO;

namespace Warehouse.Services.services
{
    public class ResourceService : IResourceService
    {
        private readonly IUnitOfWork _IUnitOfWork;
        private readonly IMapper _mapper;

        public ResourceService(IUnitOfWork IUnitOfWork, IMapper mapper)
        {
            _IUnitOfWork = IUnitOfWork;
            _mapper = mapper;
        }

        #region Get All, Get By Id
        public async Task<List<ResourceDto>> GetAllAsync()
        {
            var models = await _IUnitOfWork.repository<Resource>().GetAllAsync();
            return _mapper.Map<List<ResourceDto>>(models);
        }

        public async Task<ResourceDto?> GetByIdAsync(int id)
        {
            var model = await _IUnitOfWork.repository<Resource>().GetByIdAsync(id);
            return _mapper.Map<ResourceDto>(model);
        }

        public async Task<List<ResourceDto>> GetByStatusAsync(STATUS status)
        {
            var models = await _IUnitOfWork.repository<Resource>().Get(m => m.Status == status);
            return _mapper.Map<List<ResourceDto>>(models);
        }

        #endregion

        #region Add, Update, Change status, Delete
        public async Task<int> CreateAsync(ResourceDto dto)
        {
            var model = _mapper.Map<Resource>(dto);
            model.Status = STATUS.InWork;
            _IUnitOfWork.repository<Resource>().Add(model);
            await _IUnitOfWork.Complete();
            return model.Id;
        }

        public async Task<bool> UpdateAsync(ResourceDto dto)
        {
            var model = await _IUnitOfWork.repository<Resource>().GetByIdAsync(dto.Id);
            _mapper.Map(dto, model);

            await _IUnitOfWork.Complete();
            return true;
        }

        public async Task<DeleteResourceResult> ChangeStatusAsync(int id)
        {
            var resources = await _IUnitOfWork.repository<Resource>().Get(m => m.Id == id);
            if (!resources.Any()) return DeleteResourceResult.NotFound;
            var resource = resources.FirstOrDefault();

            resource.Status = resource.Status==STATUS.InArchive? STATUS.InWork: STATUS.InArchive;

            await _IUnitOfWork.Complete();
            return DeleteResourceResult.Success;
        }
       
        public async Task<DeleteResourceResult> DeleteAsnc(int id)
        {
            var resources = await _IUnitOfWork.repository<Resource>().Get(m => m.Id == id,
                includeProperties: "InboundItems,OutboundItems");
            if (!resources.Any()) return DeleteResourceResult.NotFound;
            var resource = resources.FirstOrDefault();

            if ( resource.InboundItems.Any() || resource.OutboundItems.Any()|| resource.Status==STATUS.InWork)
                return DeleteResourceResult.HasDependencies;

            _IUnitOfWork.repository<Resource>().Delete(resource);

            await _IUnitOfWork.Complete();
            return DeleteResourceResult.Success;
        }

        #endregion

        #region functions for validations

        public bool IsIdExist(int id)
        {
            if (id > 0)
            {
                var resources =  _IUnitOfWork.repository<Resource>().Get(m => m.Id == id);
                return resources.Result.Any();
            }
            return true;

        }

        public bool IsNameExist(int id, string name)
        {
            var resources =  _IUnitOfWork.repository<Resource>().Get(m => m.Name == name && (m.Id != id || id == 0));
            return resources.Result.Any();

        }

        #endregion
    }

}
