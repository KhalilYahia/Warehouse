
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
    public class ShipmentsService : IShipmentsService
    {
        private readonly IUnitOfWork _IUnitOfWork;
        private readonly IMapper _mapper;

        public ShipmentsService(IUnitOfWork IUnitOfWork, IMapper mapper)
        {
            _IUnitOfWork = IUnitOfWork;
            _mapper = mapper;
        }

        #region Get All, Get By Id
        public async Task<List<ShipmentDto>> Search(SearchInShipmentsDto dto)
        {
            IQueryable<OutboundDocument> query = _IUnitOfWork.repository<OutboundDocument>().GetAllAsync_AsIqueryable()
                .Include(x => x.Items)
                    .ThenInclude(m => m.Resource)
                .Include(x => x.Items)
                    .ThenInclude(z => z.Unit)
                .Include(x => x.Client);

            if (dto.OutboundDocumentIds != null && dto.OutboundDocumentIds.Any())
                query = query.Where(x => dto.OutboundDocumentIds.Contains(x.Id));

            if (dto.StartPeriod.HasValue)
                query = query.Where(x => x.Date >= dto.StartPeriod.Value);

            if (dto.EndPeriod.HasValue)
                query = query.Where(x => x.Date <= dto.EndPeriod.Value);


            if (dto.ResourceIds != null && dto.ResourceIds.Any())
                query = query.Where(x => x.Items.Any(i => dto.ResourceIds.Contains(i.ResourceId)));

            if (dto.UnitIds != null && dto.UnitIds.Any())
                query = query.Where(x => x.Items.Any(i => dto.UnitIds.Contains(i.UnitId)));

            if (dto.ClientIds != null && dto.ClientIds.Any())
                query = query.Where(x => dto.ClientIds.Contains(x.ClientId));


            var results = await query.Select(r => new ShipmentDto
            {
                Id = r.Id,
                Number = r.Number,
                Date = r.Date,
                ClientId=r.ClientId,
                IsSigned = r.IsSigned,
                ClientName = r.Client.Name,
                Goods = r.Items.Select(i => new OutboundItemDto
                {
                    Id = i.Id,
                    ResourceId = i.ResourceId,
                    Quantity = i.Quantity,
                    UnitId = i.UnitId,
                    ResourceName = i.Resource != null ? i.Resource.Name : string.Empty,
                    UnitName = i.Unit != null ? i.Unit.Name : string.Empty
                }).ToList()
            }).ToListAsync();

            return results;
        }

        public async Task<ShipmentDto?> GetByIdAsync(int InboundDocument_Id)
        {
            var models = await _IUnitOfWork.repository<OutboundDocument>().GetAllAsync_AsIqueryable()
                                                                   .Where(m => m.Id == InboundDocument_Id)
                                                                   .Include(m => m.Items).ThenInclude(r => r.Resource)
                                                                   .Include(m => m.Items).ThenInclude(r => r.Unit)
                                                                   .Include(m => m.Client).ToListAsync();
            if (models.Any())
            {
                var model = models.First();
                var result = new ShipmentDto
                {
                    Id = model.Id,
                    Date = model.Date,
                    Number = model.Number,
                    IsSigned =model.IsSigned,
                    ClientId = model.ClientId,
                    ClientName = model.Client.Name,
                    Goods = new List<OutboundItemDto>()
                };
                foreach (var item in model.Items)
                {
                    result.Goods.Add(new OutboundItemDto
                    {
                        Id = item.Id,
                        Quantity = item.Quantity,
                        ResourceId = item.ResourceId,
                        UnitId = item.UnitId,
                        
                        ResourceName = item.Resource.Name,
                        UnitName = item.Unit.Name
                    });
                }

                return result;
            }


            return null;
        }

        #endregion

        #region Add, Update, Delete
        public async Task<int> CreateAsync(ShipmentDto dto)
        {
            var model = _mapper.Map<OutboundDocument>(dto);
            model.Items= _mapper.Map<List<OutboundItem>>(dto.Goods);

            if(dto.IsSigned)
            {

                // Load all relevant stock balances at once
                var resourceUnitPairs = model.Items.Select(i => new { i.ResourceId, i.UnitId }).ToList();
                var allBalances = await _IUnitOfWork.repository<Balance>().GetAllAsync();
                var filteredBalances = allBalances.Where(b => resourceUnitPairs.Contains(new { b.ResourceId, b.UnitId })).ToList();
                var stockBalances_Dict = filteredBalances.ToDictionary(b => (b.ResourceId, b.UnitId));

                foreach (var item in model.Items)
                {
                    if (stockBalances_Dict.TryGetValue((item.ResourceId, item.UnitId), out var stock) && stock.Quantity >= item.Quantity)
                    {
                        stock.Quantity -= item.Quantity;
                    }
                    else
                    {
                        throw new InvalidOperationException($"Stock for ResourceId {item.ResourceId} not found.");
                    }
                }

            }

            _IUnitOfWork.repository<OutboundDocument>().Add(model);
            try
            {
                await _IUnitOfWork.Complete();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new Exception("The document was modified by another user. Please reload and try again.");
            }
            return model.Id;
        }

        public async Task<bool> UpdateAsync(ShipmentDto dto)
        {
            var Shipments = await _IUnitOfWork.repository<OutboundDocument>().Get(m=>m.Id==dto.Id,
                                    includeProperties: "Items");

            var Shipment = Shipments.FirstOrDefault();
            Shipment.Number = dto.Number;
            Shipment.Date = dto.Date;
            Shipment.ClientId = dto.ClientId;

          
            // Remove items not in the DTO
            var dtoItemIds = dto.Goods.Where(i => i.Id>0).Select(i => i.Id).ToList();
            var itemsToRemove = Shipment.Items.Where(i => !dtoItemIds.Contains(i.Id)).ToList();
            foreach (var item in itemsToRemove)
            {
                Shipment.Items.Remove(item);
               // _IUnitOfWork.repository<OutboundItem>().DeleteAsync(item); 
            }


            // Update existing or add new
            foreach (var dtoItem in dto.Goods)
            {
                
                var existingItem = Shipment.Items.FirstOrDefault(i => i.Id == dtoItem.Id);
                if(existingItem!=null)
                {
                    existingItem.UnitId = dtoItem.UnitId;
                    existingItem.Quantity = dtoItem.Quantity;
                    existingItem.ResourceId = dtoItem.ResourceId;
                }
                else
                {
                    var newItem = new OutboundItem
                    {
                        ResourceId = dtoItem.ResourceId,
                        Quantity = dtoItem.Quantity,
                        UnitId = dtoItem.UnitId

                    };
                    Shipment.Items.Add(newItem);
                }
                    
              
            }

            if (dto.IsSigned)
            {
                // Load all relevant stock balances at once
                var resourceUnitPairs = Shipment.Items.Select(i => new { i.ResourceId, i.UnitId }).ToList();
                var allBalances = await _IUnitOfWork.repository<Balance>().GetAllAsync();
                var filteredBalances = allBalances.Where(b => resourceUnitPairs.Contains(new { b.ResourceId, b.UnitId })).ToList();
                var stockBalances_Dict = filteredBalances.ToDictionary(b => (b.ResourceId, b.UnitId));
                               
                foreach (var item in Shipment.Items)
                {
                    if (stockBalances_Dict.TryGetValue((item.ResourceId, item.UnitId), out var stock) && stock.Quantity >= item.Quantity)
                    {                                               
                        stock.Quantity -= item.Quantity;
                    }
                    else
                    {
                        throw new InvalidOperationException($"Stock for ResourceId {item.ResourceId} not found.");
                    }
                }
            }

            try
            {
                _IUnitOfWork.repository<OutboundDocument>().UpdateAsync(Shipment);
                await _IUnitOfWork.Complete();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new Exception("The document was modified by another user. Please reload and try again.");
            }
            return true;
        }
   
        public async Task<DeleteResourceResult> UnSign(int OutboundDocumentId)
        {
            var models = await _IUnitOfWork.repository<OutboundDocument>().Get(m => m.Id == OutboundDocumentId, includeProperties: "Items");
            if (!models.Any()) return DeleteResourceResult.NotFound;
            var model = models.First();
            if (model.IsSigned)
            {
                model.IsSigned = false;
                // Load all relevant stock balances at once
                var resourceUnitPairs = model.Items.Select(i => new { i.ResourceId, i.UnitId }).ToList();
                var allBalances = await _IUnitOfWork.repository<Balance>().GetAllAsync();
                var filteredBalances = allBalances.Where(b => resourceUnitPairs.Contains(new { b.ResourceId, b.UnitId })).ToList();
                var stockBalances_Dict = filteredBalances.ToDictionary(b => (b.ResourceId, b.UnitId));

                foreach (var item in model.Items)
                {
                    if (stockBalances_Dict.TryGetValue((item.ResourceId,item.UnitId), out var stock))
                    {
                        stock.Quantity += item.Quantity;
                    }
                    else
                    {
                        throw new InvalidOperationException($"Stock for ResourceId {item.ResourceId} not found.");
                    }

                }
                try
                {
                    _IUnitOfWork.repository<OutboundDocument>().UpdateAsync(model);
                    await _IUnitOfWork.Complete();
                }
                catch (DbUpdateConcurrencyException)
                {
                    throw new Exception("The document was modified by another user. Please reload and try again.");
                }
                return DeleteResourceResult.Success;
            }
            return DeleteResourceResult.HasDependencies;
        }
        public async Task<DeleteResourceResult> DeleteAsnc(int id)
        {
            var models = await _IUnitOfWork.repository<OutboundDocument>().Get(m => m.Id == id, includeProperties: "Items");
            if (!models.Any()) return DeleteResourceResult.NotFound;
            var model = models.First();

            if (model.IsSigned)
                return DeleteResourceResult.HasDependencies;

            // Delete related items
            foreach (var item in model.Items.ToList())
            {
                _IUnitOfWork.repository<OutboundItem>().Delete(item);
            }

            _IUnitOfWork.repository<OutboundDocument>().Delete(model);

            await _IUnitOfWork.Complete();
            return DeleteResourceResult.Success;
        }

        #endregion

        #region functions for validations

        public bool IsIdExist(int id)
        {
            if (id > 0)
            {
                var Models =  _IUnitOfWork.repository<OutboundDocument>().Get(m => m.Id == id);
                return Models.Result.Any();
            }
            return true;

        }
        public bool IsSigned(int id)
        {
            if (id > 0)
            {
                var Models = _IUnitOfWork.repository<OutboundDocument>().Get(m => m.Id == id && m.IsSigned);
                return Models.Result.Any();
            }
            return true;

        }

        public bool IsResourceExistAndActive(int ResourceId)
        {
            
            var Models =  _IUnitOfWork.repository<Resource>().Get(m => m.Id== ResourceId && m.Status == STATUS.InWork);
            return Models.Result.Any();       

        }

        public bool IsUnitExistAndActive(int UnitId)
        {

            var Models =  _IUnitOfWork.repository<Unit>().Get(m => m.Id== UnitId && m.Status==STATUS.InWork);
            return Models.Result.Any();

        }

        public bool IsClientExistAndActive(int ClientId)
        {

            var Models =  _IUnitOfWork.repository<Client>().Get(m => m.Id == ClientId && m.Status == STATUS.InWork);
            return Models.Result.Any();

        }

        #endregion
    }

}
