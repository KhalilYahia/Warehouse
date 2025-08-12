
using AutoMapper;
using Microsoft.AspNetCore.Hosting;
using LLama.Common;
using LLama;
using Warehouse.Common;
using Warehouse.Domain.Entities;
using Warehouse.Services.Iservices;
using Warehouse.Domain;
using Warehouse.Services.DTO;
using Castle.Core.Resource;

namespace Warehouse.Services.services
{
    public class ClientService : IClientService
    {
        private readonly IUnitOfWork _IUnitOfWork;
        private readonly IMapper _mapper;

        public ClientService(IUnitOfWork IUnitOfWork, IMapper mapper)
        {
            _IUnitOfWork = IUnitOfWork;
            _mapper = mapper;
        }

        #region Get All, Get By Id
        public async Task<List<ClientDto>> GetAllAsync()
        {
            var models = await _IUnitOfWork.repository<Client>().GetAllAsync();
            return _mapper.Map<List<ClientDto>>(models);
        }
        public async Task<List<ClientDto>> GetByStatusAsync(STATUS status)
        {
            var models = await _IUnitOfWork.repository<Client>().Get(m=>m.Status==status);
            return _mapper.Map<List<ClientDto>>(models);
        }

        public async Task<ClientDto?> GetByIdAsync(int id)
        {
            var model = await _IUnitOfWork.repository<Client>().GetByIdAsync(id);
            return _mapper.Map<ClientDto>(model);
        }

        #endregion

        #region Add, Update, Change status, Delete
        public async Task<int> CreateAsync(ClientDto dto)
        {
            var model = _mapper.Map<Client>(dto);
            model.Status = STATUS.InWork;
            _IUnitOfWork.repository<Client>().Add(model);
            await _IUnitOfWork.Complete();
            return model.Id;
        }

        public async Task<bool> UpdateAsync(ClientDto dto)
        {
            var model = await _IUnitOfWork.repository<Client>().GetByIdAsync(dto.Id);
            _mapper.Map(dto, model);

            await _IUnitOfWork.Complete();
            return true;
        }

        public async Task<DeleteResourceResult> ChangeStatusAsync(int id)
        {
            var models = await _IUnitOfWork.repository<Client>().Get(m => m.Id == id);
            if (!models.Any()) return DeleteResourceResult.NotFound;
            var model = models.FirstOrDefault();

            model.Status = model.Status==STATUS.InArchive? STATUS.InWork: STATUS.InArchive;

            await _IUnitOfWork.Complete();
            return DeleteResourceResult.Success;
        }
       
        public async Task<DeleteResourceResult> DeleteAsnc(int id)
        {
            var models = await _IUnitOfWork.repository<Client>().Get(m => m.Id == id,
                includeProperties: "OutboundDocuments");
            if (!models.Any()) return DeleteResourceResult.NotFound;
            var model = models.FirstOrDefault();

            if (model.OutboundDocuments.Any()|| model.Status == STATUS.InWork)
                return DeleteResourceResult.HasDependencies;

            _IUnitOfWork.repository<Client>().Delete(model);

            await _IUnitOfWork.Complete();
            return DeleteResourceResult.Success;
        }

        #endregion

        #region functions for validations

        public bool IsIdExist(int id)
        {
            if (id > 0)
            {
                var Models =  _IUnitOfWork.repository<Client>().Get(m => m.Id == id);
                return Models.Result.Any();
            }
            return true;

        }

        public bool IsNameExist(int id,string name)
        {
            var models =  _IUnitOfWork.repository<Client>().Get(m => m.Name == name && (m.Id!=id || id == 0));
            return models.Result.Any();

        }

        #endregion
    }

}
