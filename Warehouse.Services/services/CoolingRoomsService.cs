using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Warehouse.Domain.Entities;
using Warehouse.Domain;
using Warehouse.Services.DTO;
using Microsoft.Extensions.Hosting;
using System.Text.RegularExpressions;
using Warehouse.Services.Iservices;
using System.Transactions;

namespace Warehouse.Services.services
{
    public class CoolingRoomsService : ICoolingRoomsService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CoolingRoomsService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        #region Add
        public async Task<int> AddAsync(InputCoolingRoomsDto inDto)
        {
            if (inDto == null) throw new ArgumentNullException(nameof(inDto));
            using (var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    var CoolingRooms = _mapper.Map<CoolingRooms>(inDto);
                    // Check if the repository exists
                    var repository = (await _unitOfWork.repository<RepositoryMaterials>().Get(m => m.Id == inDto.RepositoryMaterialId)).FirstOrDefault();
                    if (repository == null)
                        throw new KeyNotFoundException("RepositoryMaterial id not found");

                    CoolingRooms.RepositoryMaterialId = repository.Id;
                    CoolingRooms.MaterialName = repository.Name;
                    CoolingRooms.CodeNumber = " ";


                    // Calculate prices
                    CoolingRooms.CostOfAll = CoolingRooms.Weight * inDto.CostOfUnit;


                    // Check if the buyer exists or add a new buyer if not
                    if (await GetOrCreateBuyerAsync(inDto, CoolingRooms))
                    {
                        if (await AddMoneytoFund(CoolingRooms.CostOfAll))
                        {

                            _unitOfWork.repository<CoolingRooms>().Add(CoolingRooms);
                            await _unitOfWork.Complete();
                            
                            transactionScope.Complete(); // Commit transaction

                            return CoolingRooms.Id;
                        }
                    }
                    return 0; // error
                }
                catch (Exception)
                {
                    // If anything fails, the transaction will be rolled back
                    throw;
                }
            }
        }

        private async Task<bool> GetOrCreateBuyerAsync(InputCoolingRoomsDto inDto, CoolingRooms inOut)
        {
            if (string.IsNullOrWhiteSpace(inDto.ClientName))
                throw new ArgumentException("Buyer name cannot be null or empty");

            var BuyerNameWithoutSpaces_ = Regex.Replace(inDto.ClientName, @"\s+", "");
            var Buyer = (await _unitOfWork.repository<Buyers>().Get(m => m.BuyerNameWithoutSpaces == BuyerNameWithoutSpaces_)).FirstOrDefault();
           
            if (Buyer != null)
            {
                inOut.ClientId = Buyer.Id;
                inOut.ClientName = Buyer.Name;

               // res_ = await UpdateSupplierFinancialEntitlementsAsync(supplier, inDto, inOut);
            }
            else
            {
                Buyer = new Buyers
                {
                    Name = inOut.ClientName,
                    BuyerNameWithoutSpaces = BuyerNameWithoutSpaces_
                };
                //res_ = await AddSupplierFinancialEntitlementsAsync(supplier, inDto, inOut);

                _unitOfWork.repository<Buyers>().Add(Buyer);
                await _unitOfWork.Complete();
            }
            inOut.ClientId = Buyer.Id;
            inOut.ClientName = Buyer.Name;
            return true;
        }

        private async Task<bool> AddMoneytoFund(decimal money)
        {
            // Find the existing entity by id
            var existingTotalFunds = (await _unitOfWork.repository<Resource>().GetAllAsync()).FirstOrDefault();
            if (existingTotalFunds == null)
            {
                return false; // Entity not found
            }

            // Update the properties
            existingTotalFunds.TotalIn += money;
            existingTotalFunds.Profits = existingTotalFunds.TotalIn - existingTotalFunds.TotalOut;
            existingTotalFunds.CurrentFund += money;

            _unitOfWork.repository<Resource>().UpdateAsync(existingTotalFunds);
            // Save changes to the database
            await _unitOfWork.Complete();

            return true; // Update successful
        }

        #endregion

        public async Task<bool> DeleteAsync(int id)
        {
            using (var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    var CoolingRooms = await _unitOfWork.repository<CoolingRooms>().GetByIdAsync(id);
            if (CoolingRooms == null)
            {
                return false;
            }
            if (await AddMoneytoFund(-(CoolingRooms.CostOfAll)))
            {
                _unitOfWork.repository<CoolingRooms>().Delete(CoolingRooms);
                await _unitOfWork.Complete();

                transactionScope.Complete(); // Commit transaction

                return true;
            }
           

            
            return false; // error when remove money from fund

        }

            catch (Exception)
            {
                // If an exception is thrown, it will rollback automatically
                throw;
            }
         }
        }

        public async Task<List<CoolingRoomsSimplifyDto>> GetAllAsync()
        {
            var CoolingRoomss = await _unitOfWork.repository<CoolingRooms>().GetAllAsync();
            return _mapper.Map<List<CoolingRoomsSimplifyDto>>(CoolingRoomss.ToList());
        }

        public async Task<CoolingRoomsDetailsDto> GetByIdAsync(int id)
        {
            var CoolingRooms = (await _unitOfWork.repository<CoolingRooms>().Get(m => m.Id == id)).FirstOrDefault();
            return _mapper.Map<CoolingRoomsDetailsDto>(CoolingRooms);
        }

        public async Task<List<CoolingRoomsDetailsDto>> GetAllAsync_Fordesktop()
        {
            var CoolingRoomss = await _unitOfWork.repository<CoolingRooms>().GetAllAsync();
            return _mapper.Map<List<CoolingRoomsDetailsDto>>(CoolingRoomss.ToList());
        }

    }

}
