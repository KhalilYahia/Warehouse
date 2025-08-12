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
    public class OtherSalesService : IOtherSalesService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public OtherSalesService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        #region Add
        public async Task<int> AddAsync(InputOtherSalesDto inDto)
        {
            if (inDto == null) throw new ArgumentNullException(nameof(inDto));

            using (var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {

                    var OtherSales = _mapper.Map<OtherSales>(inDto);
                    // Check if the repository exists
                    var repository = (await _unitOfWork.repository<RepositoryMaterials>().Get(m => m.Id == inDto.RepositoryMaterialId)).FirstOrDefault();
                    if (repository == null)
                        throw new KeyNotFoundException("repositorymaterial id not found");

                    OtherSales.RepositoryMaterialId = repository.Id;
                    OtherSales.MaterialName = repository.Name;
                    // Calculate prices
                    //OtherSales.CostOfAll = OtherSales.Weight * inDto.CostOfUnit;


                    // Check if the buyer exists or add a new buyer if not
                    if (await GetOrCreateBuyerAsync(inDto, OtherSales))
                    {
                        if (await AddMoneytoFund(OtherSales.SalesPriceOfAll))
                        {

                            _unitOfWork.repository<OtherSales>().Add(OtherSales);
                            await _unitOfWork.Complete();

                            transactionScope.Complete(); // Commit transaction

                            return OtherSales.Id;
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

        private async Task<bool> GetOrCreateBuyerAsync(InputOtherSalesDto inDto, OtherSales inOut)
        {
            if (string.IsNullOrWhiteSpace(inDto.BuyerName))
                throw new ArgumentException("Buyer name cannot be null or empty");

            var BuyerNameWithoutSpaces_ = Regex.Replace(inDto.BuyerName, @"\s+", "");
            var Buyer = (await _unitOfWork.repository<Buyers>().Get(m => m.BuyerNameWithoutSpaces == BuyerNameWithoutSpaces_)).FirstOrDefault();
           
            if (Buyer != null)
            {
                inOut.BuyerId = Buyer.Id;
                inOut.BuyerName = Buyer.Name;

               // res_ = await UpdateSupplierFinancialEntitlementsAsync(supplier, inDto, inOut);
            }
            else
            {
                Buyer = new Buyers
                {
                    Name = inOut.BuyerName,
                    BuyerNameWithoutSpaces = BuyerNameWithoutSpaces_
                };
                //res_ = await AddSupplierFinancialEntitlementsAsync(supplier, inDto, inOut);

                _unitOfWork.repository<Buyers>().Add(Buyer);
                await _unitOfWork.Complete();
            }
            inOut.BuyerId = Buyer.Id;
            inOut.BuyerName = Buyer.Name;
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
                    var OtherSales = await _unitOfWork.repository<OtherSales>().GetByIdAsync(id);
                    if (OtherSales == null)
                    {
                        return false;
                    }
                    if (await AddMoneytoFund(-(OtherSales.SalesPriceOfAll)))
                    {
                        _unitOfWork.repository<OtherSales>().Delete(OtherSales);
                        await _unitOfWork.Complete();

                        transactionScope.Complete(); // Commit transaction

                        return true;
                    }
                    return false; // error when remove money from fund
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

        public async Task<List<OtherSalesSimplifyDto>> GetAllAsync()
        {
            var OtherSaless = await _unitOfWork.repository<OtherSales>().GetAllAsync();
            return _mapper.Map<List<OtherSalesSimplifyDto>>(OtherSaless.ToList());
        }

        public async Task<OtherSalesDetailsDto> GetByIdAsync(int id)
        {
            var OtherSales = (await _unitOfWork.repository<OtherSales>().Get(m => m.Id == id)).FirstOrDefault();
            return _mapper.Map<OtherSalesDetailsDto>(OtherSales);
        }

        public async Task<List<OtherSalesDetailsDto>> GetAllAsync_Fordesktop()
        {
            var OtherSaless = await _unitOfWork.repository<OtherSales>().GetAllAsync();
            return _mapper.Map<List<OtherSalesDetailsDto>>(OtherSaless.ToList());
        }


    }

}
