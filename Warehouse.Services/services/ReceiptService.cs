
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
    public class ReceiptService : IReceiptService
    {
        private readonly IUnitOfWork _IUnitOfWork;
        private readonly IMapper _mapper;

        public ReceiptService(IUnitOfWork IUnitOfWork, IMapper mapper)
        {
            _IUnitOfWork = IUnitOfWork;
            _mapper = mapper;
        }

        #region Get All, Get By Id
        public async Task<List<ReceiptDto>> Search(SearchInReceiptsDto dto)
        {
            IQueryable<InboundDocument> query =  _IUnitOfWork.repository<InboundDocument>().GetAllAsync_AsIqueryable()
                .Include(x=>x.Items)
                    .ThenInclude(m=>m.Resource)
                .Include(x=>x.Items)
                    .ThenInclude(z=>z.Unit);

            if (dto.InboundDocumentIds != null && dto.InboundDocumentIds.Any())
                query = query.Where(x => dto.InboundDocumentIds.Contains(x.Id));

            if (dto.StartPeriod.HasValue)
                query = query.Where(x => x.Date >= dto.StartPeriod.Value);

            if (dto.EndPeriod.HasValue)
                query = query.Where(x => x.Date <= dto.EndPeriod.Value);


            if (dto.ResourceIds != null && dto.ResourceIds.Any())
                query = query.Where(x => x.Items.Any(i => dto.ResourceIds.Contains(i.ResourceId)));

            if (dto.UnitIds != null && dto.UnitIds.Any())
                query = query.Where(x => x.Items.Any(i => dto.UnitIds.Contains(i.UnitId)));

            Console.WriteLine(query.ToQueryString());
            var results = await query.Select(r => new ReceiptDto
            {
                Id = r.Id,
                Number = r.Number,
                Date = r.Date,
                Goods = r.Items.Select(i => new InboundItemDto
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


        public async Task<ReceiptDto?> GetByIdAsync(int InboundDocument_Id)
        {
            var models = await  _IUnitOfWork.repository<InboundDocument>().GetAllAsync_AsIqueryable()
                                                                   .Where(m=>m.Id==InboundDocument_Id)
                                                                   .Include(m=>m.Items).ThenInclude(r=>r.Resource)
                                                                   .Include(m => m.Items).ThenInclude(r => r.Unit)
                                                                   .ToListAsync();
            if(models.Any())
            {
                var model = models.First();
                var result = new ReceiptDto
                {
                    Id = model.Id,
                    Date=model.Date,
                    Number=model.Number,
                    Goods= new List<InboundItemDto>()
                };
                foreach(var item in model.Items)
                {
                    result.Goods.Add(new InboundItemDto
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


        public async Task<FilterReceiptRequiresDto> GetAllFitters()
        {
            var models_DocNum = await _IUnitOfWork.repository<InboundDocument>().GetAllAsync();
            var models_Resources = await _IUnitOfWork.repository<Resource>().Get(m => m.Status == STATUS.InWork);
            var models_Units = await _IUnitOfWork.repository<Unit>().Get(m => m.Status == STATUS.InWork);
            var models_Clients = await _IUnitOfWork.repository<Client>().Get(m => m.Status == STATUS.InWork);

            var result = new FilterReceiptRequiresDto
            {
                DocumentsNumber = _mapper.Map<List<InboundDocDto>>(models_DocNum),
                Resources = _mapper.Map<List<ResourceDto>>(models_Resources),
                Units = _mapper.Map<List<UnitDto>>(models_Units),
                Clients = _mapper.Map<List<ClientDto>>(models_Clients),

            };

            return result;

        }

        #endregion

        #region Add, Update, Change status, Delete
        public async Task<int> CreateAsync(ReceiptDto dto)
        {
            var model = _mapper.Map<InboundDocument>(dto);
            model.Items= _mapper.Map<List<InboundItem>>(dto.Goods);

            if (model.Items.Any())
            {
                // Load all relevant stock balances at once
                var resourceUnitPairs = model.Items.Select(i => new { i.ResourceId, i.UnitId }).ToList();
                var allBalances = await _IUnitOfWork.repository<Balance>().GetAllAsync();
                var filteredBalances = allBalances.Where(b => resourceUnitPairs.Contains(new { b.ResourceId, b.UnitId })).ToList();
                var stockBalances_Dict = filteredBalances.ToDictionary(b => (b.ResourceId, b.UnitId));

                foreach (var item in model.Items)
                {
                    if (stockBalances_Dict.TryGetValue((item.ResourceId, item.UnitId), out var stock))
                    {
                        stock.Quantity += item.Quantity;
                    }
                    else
                    {
                        // Add to dictionary and context if it's a new stock record
                        stock = new Balance
                        {
                            ResourceId = item.ResourceId,
                            UnitId = item.UnitId,
                            Quantity = item.Quantity
                        };
                        stockBalances_Dict[(item.ResourceId, item.UnitId)] = stock;

                        _IUnitOfWork.repository<Balance>().Add(stock);
                    }
                }
            }
            _IUnitOfWork.repository<InboundDocument>().Add(model);
            
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

        public async Task<bool> UpdateAsync(ReceiptDto dto)
        {
            var receipts = await _IUnitOfWork.repository<InboundDocument>().Get(m=>m.Id==dto.Id,
                                    includeProperties: "Items");

            var receipt = receipts.FirstOrDefault();
            receipt.Number = dto.Number;
            receipt.Date = dto.Date;

            // Load all relevant stock balances at once
            var resourceUnitPairs = receipt.Items.Select(i => new { i.ResourceId, i.UnitId }).ToList();
            var allBalances = await _IUnitOfWork.repository<Balance>().GetAllAsync();
            var filteredBalances = allBalances.Where(b => resourceUnitPairs.Contains(new { b.ResourceId, b.UnitId })).ToList();
            var stockBalances_Dict = filteredBalances.ToDictionary(b => (b.ResourceId, b.UnitId));

            // Reverse Old Quantities in Memory
            foreach (var item in receipt.Items)
            {
                if (stockBalances_Dict.TryGetValue((item.ResourceId, item.UnitId), out var stock))
                {
                    stock.Quantity -= item.Quantity;
                    
                }
            }

            // Remove items not in the DTO
            var dtoItemIds = dto.Goods.Where(i => i.Id>0).Select(i => i.Id).ToList();
            var itemsToRemove = receipt.Items.Where(i => !dtoItemIds.Contains(i.Id)).ToList();
            foreach (var item in itemsToRemove)
            {
                receipt.Items.Remove(item);
            }


            // Update existing or add new
            foreach (var dtoItem in dto.Goods)
            {
                
                var existingItem = receipt.Items.FirstOrDefault(i => i.Id == dtoItem.Id);
                if(existingItem!=null)
                {
                    existingItem.UnitId = dtoItem.UnitId;
                    existingItem.Quantity = dtoItem.Quantity;
                    existingItem.ResourceId = dtoItem.ResourceId;
                }
                else
                {
                    var newItem = new InboundItem
                    {
                        ResourceId = dtoItem.ResourceId,
                        Quantity = dtoItem.Quantity,
                        UnitId = dtoItem.UnitId

                    };
                    receipt.Items.Add(newItem);
                }
                    
              
            }

            // ReLoad all relevant stock balances at once
            var resourceUnitPairs_newItems = receipt.Items.Select(i => new { i.ResourceId, i.UnitId }).ToList();
            resourceUnitPairs_newItems.RemoveAll(m => stockBalances_Dict.ContainsKey(( m.ResourceId, m.UnitId)));
            var InBalance_NotInOld_Dic = allBalances.Where(b => resourceUnitPairs_newItems.Contains(new { b.ResourceId, b.UnitId })).ToList();
            var stockBalances_newDict = InBalance_NotInOld_Dic.ToDictionary(b => (b.ResourceId, b.UnitId));
            //  Reapply New Quantities
            foreach (var item in receipt.Items)
            {
                if (stockBalances_Dict.TryGetValue((item.ResourceId, item.UnitId), out var stock))
                {
                    stock.Quantity += item.Quantity;

                    _IUnitOfWork.repository<Balance>().Update(stock);
                }
                else if(stockBalances_newDict.TryGetValue((item.ResourceId, item.UnitId), out var newstock))
                {
                    newstock.Quantity += item.Quantity;
                    _IUnitOfWork.repository<Balance>().Update(newstock);
                }
                else
                {
                    // Add to dictionary and context if it's a new stock record
                    stock = new Balance
                    {
                        ResourceId = item.ResourceId,
                        UnitId = item.UnitId,
                        Quantity = item.Quantity
                    };
                   // stockBalances_Dict[(item.ResourceId, item.UnitId)] = stock;

                    _IUnitOfWork.repository<Balance>().Add(stock);
                }
            }

            try
            {
                _IUnitOfWork.repository<InboundDocument>().UpdateAsync(receipt);
                await _IUnitOfWork.Complete();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new Exception("The document was modified by another user. Please reload and try again.");
            }
            return true;
        }
   
        public async Task<DeleteResourceResult> DeleteAsnc(int id)
        {
            var models = await _IUnitOfWork.repository<InboundDocument>().Get(m => m.Id == id, includeProperties: "Items");
            if (!models.Any()) return DeleteResourceResult.NotFound;
            var model = models.First();

            // Load all relevant stock balances at once
            var resourceUnitPairs = model.Items.Select(i => new { i.ResourceId, i.UnitId }).ToList();
            var allBalances = await _IUnitOfWork.repository<Balance>().GetAllAsync();
            var filteredBalances = allBalances.Where(b => resourceUnitPairs.Contains(new { b.ResourceId, b.UnitId })).ToList();
            var stockBalances_Dict = filteredBalances.ToDictionary(b => (b.ResourceId, b.UnitId));

            foreach (var item in model.Items)
            {
                if (stockBalances_Dict.TryGetValue((item.ResourceId, item.UnitId), out var stock))
                {
                    stock.Quantity -= item.Quantity;
                }
            }

            // Delete related items
            foreach (var item in model.Items.ToList())
            {
                _IUnitOfWork.repository<InboundItem>().Delete(item);
            }

            _IUnitOfWork.repository<InboundDocument>().Delete(model);

            try
            {
                await _IUnitOfWork.Complete();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new Exception("The document was modified by another user. Please reload and try again.");
            }
            return DeleteResourceResult.Success;
        }

        #endregion

        #region functions for validations

        public bool IsIdExist(int id)
        {
            if (id > 0)
            {
                var Models = _IUnitOfWork.repository<InboundDocument>().Get(m => m.Id == id);
                return Models.Result.Any();
            }
            return true;

        }
       

        public bool IsResourcesExistAndActive(int ResourceId)
        {
            
            var Models =  _IUnitOfWork.repository<Resource>().Get(m => m.Id== ResourceId && m.Status == STATUS.InWork);
            return Models.Result.Any();       

        }

        public bool IsUnitssExistAndActive(int UnitId)
        {

            var Models = _IUnitOfWork.repository<Unit>().Get(m => m.Id== UnitId && m.Status==STATUS.InWork);
            return Models.Result.Any();

        }

        public bool IsInboundDocNumberExist(int id, string number)
        {

            var Models = _IUnitOfWork.repository<InboundDocument>().Get(m => m.Number == number && m.Id!=id);
            return Models.Result.Any();

        }


        #endregion
    }

}
