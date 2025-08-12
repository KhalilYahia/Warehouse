
using AutoMapper;
using Microsoft.AspNetCore.Hosting;
using LLama.Common;
using LLama;
using Warehouse.Common;
using Warehouse.Domain.Entities;
using Warehouse.Services.Iservices;
using Warehouse.Domain;
using Warehouse.Services.DTO;
using Microsoft.EntityFrameworkCore;

namespace Warehouse.Services.services
{
    public class BalanceService : IBalanceService
    {
        private readonly IUnitOfWork _IUnitOfWork;
        private readonly IMapper _mapper;

        public BalanceService(IUnitOfWork IUnitOfWork, IMapper mapper)
        {
            _IUnitOfWork = IUnitOfWork;
            _mapper = mapper;
        }

      
        public async Task<List<BalanceDto>> Search(SearchInBalanceDto dto)
        {
            IQueryable<Balance> query = _IUnitOfWork.repository<Balance>().GetAllAsync_AsIqueryable()
                .Include(x => x.Resource)
                .Include(x => x.Unit);


            if (dto.ResourceIds != null && dto.ResourceIds.Any())
                query = query.Where(x => dto.ResourceIds.Contains(x.ResourceId));

            if (dto.UnitIds != null && dto.UnitIds.Any())
                query = query.Where(x => dto.UnitIds.Contains(x.UnitId));

            
            var results = await query.Select(r => new BalanceDto
            {
                Id = r.Id,
                Quantity = r.Quantity,
                UnitId = r.UnitId,
                ResourceId =r.ResourceId,
                ResourceName = r.Resource.Name,
                UnitName=r.Unit.Name
            }).ToListAsync();

            return results;
        }

        public async Task<AllActiveElementsDto> GetAllActiveElements ()
        {
            var models_Clients = await  _IUnitOfWork.repository<Client>().Get(m => m.Status == STATUS.InWork);
            var models_Resources = await _IUnitOfWork.repository<Resource>().Get(m => m.Status == STATUS.InWork);
            var models_Units = await _IUnitOfWork.repository<Unit>().Get(m => m.Status == STATUS.InWork);


            var result = new AllActiveElementsDto
            {
                Clients = _mapper.Map<List<ClientDto>>(models_Clients),
                Resources = _mapper.Map<List<ResourceDto>>(models_Resources),
                Units = _mapper.Map<List<UnitDto>>(models_Units)
            };

            return result;

        }

        public async Task<List<BalanceDto>> GetAllActivatedInBalance()
        {
            IQueryable<Balance> query = _IUnitOfWork.repository<Balance>().GetAllAsync_AsIqueryable()
                .Include(x => x.Resource).Where(x=>x.Resource.Status == STATUS.InWork)
                .Include(x => x.Unit).Where(x => x.Unit.Status == STATUS.InWork);

            var results = await query.Select(r => new BalanceDto
            {
                Id = r.Id,
                Quantity = r.Quantity,
                UnitId = r.UnitId,
                ResourceId = r.ResourceId,
                ResourceName = r.Resource.Name,
                UnitName = r.Unit.Name
            }).ToListAsync();

            return results;
        }

    }

}
