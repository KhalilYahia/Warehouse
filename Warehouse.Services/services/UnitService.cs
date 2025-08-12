
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
    public class UnitService : IUnitService
    {
        private readonly IUnitOfWork _IUnitOfWork;
        private readonly IMapper _mapper;

        public UnitService(IUnitOfWork IUnitOfWork, IMapper mapper)
        {
            _IUnitOfWork = IUnitOfWork;
            _mapper = mapper;
        }

        #region Get All, Get By Id
        public async Task<List<UnitDto>> GetAllAsync()
        {
            var models = await _IUnitOfWork.repository<Unit>().GetAllAsync();
            return _mapper.Map<List<UnitDto>>(models);
        }

        public async Task<UnitDto?> GetByIdAsync(int id)
        {
            var model = await _IUnitOfWork.repository<Unit>().GetByIdAsync(id);
            return _mapper.Map<UnitDto>(model);
        }

        public async Task<List<UnitDto>> GetByStatusAsync(STATUS status)
        {
            var models = await _IUnitOfWork.repository<Unit>().Get(m => m.Status == status);
            return _mapper.Map<List<UnitDto>>(models);
        }

        #endregion

        #region Add, Update, Change status, Delete
        public async Task<int> CreateAsync(UnitDto dto)
        {
            var model = _mapper.Map<Unit>(dto);
            model.Status = STATUS.InWork;
            _IUnitOfWork.repository<Unit>().Add(model);
            await _IUnitOfWork.Complete();
            return model.Id;
        }

        public async Task<bool> UpdateAsync(UnitDto dto)
        {
            var model = await _IUnitOfWork.repository<Unit>().GetByIdAsync(dto.Id);
            _mapper.Map(dto, model);

            await _IUnitOfWork.Complete();
            return true;
        }

        public async Task<DeleteResourceResult> ChangeStatusAsync(int id)
        {
            var models = await _IUnitOfWork.repository<Unit>().Get(m => m.Id == id);
            if (!models.Any()) return DeleteResourceResult.NotFound;
            var model = models.FirstOrDefault();

            model.Status = model.Status==STATUS.InArchive? STATUS.InWork: STATUS.InArchive;

            await _IUnitOfWork.Complete();
            return DeleteResourceResult.Success;
        }
       
        public async Task<DeleteResourceResult> DeleteAsnc(int id)
        {
            var models = await _IUnitOfWork.repository<Unit>().Get(m => m.Id == id,
                includeProperties: "InboundItems,OutboundItems");
            if (!models.Any()) return DeleteResourceResult.NotFound;
            var model = models.FirstOrDefault();

            if ( model.InboundItems.Any() || model.OutboundItems.Any() || model.Status == STATUS.InWork)
                return DeleteResourceResult.HasDependencies;

            _IUnitOfWork.repository<Unit>().Delete(model);

            await _IUnitOfWork.Complete();
            return DeleteResourceResult.Success;
        }

        #endregion

        #region functions for validations

        public bool IsIdExist(int id)
        {
            if (id > 0)
            {
                var Models = _IUnitOfWork.repository<Unit>().Get(m => m.Id == id);
                return Models.Result.Any();
            }
            return true;

        }

        public bool IsNameExist(int id,string name)
        {
            
            var models =  _IUnitOfWork.repository<Unit>().Get(m => m.Name == name && (m.Id!=id || id==0));
            return models.Result.Any();

        }

        #endregion
    }

}
