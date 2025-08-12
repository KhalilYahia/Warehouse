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
    public class RefrigeratorService : IRefrigeratorService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public RefrigeratorService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        #region Add
        public async Task<int> AddAsync(InputRefrigeratorDto inDto)
        {
            if (inDto == null) throw new ArgumentNullException(nameof(inDto));
            if ((inDto.Details.Count()==0)|| (inDto.Details==null)) throw new ArgumentNullException(nameof(inDto));
            using (var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    var refrigerator = _mapper.Map<Refrigerator>(inDto);
                    refrigerator.RefrigeratorDetails = _mapper.Map<List<RefrigeratorDetails>>(inDto.Details);

                    RepositoryMaterials repository;
                    foreach (var item in refrigerator.RefrigeratorDetails)
                    {
                        repository = (await _unitOfWork.repository<RepositoryMaterials>().Get(m => m.Id == item.RepositoryMaterialId)).FirstOrDefault();
                        if (repository == null)
                            throw new KeyNotFoundException("RepositoryMaterial id not found");

                        item.RepositoryMaterialId = repository.Id;
                        item.MaterialName = repository.Name;

                        item.WeightAfterDiscount_2Percent = (item.BalanceCardWeight - (item.CountOfBoxes * item.EmptyBoxesWeight)) * ((decimal)0.98);
                        // Calculate prices
                        item.SalesPriceOfAll = item.WeightAfterDiscount_2Percent * item.SalesPriceOfUnit;

                    }
                    // Check if the repository exists


                    refrigerator.CodeNumber = " ";
                    refrigerator.TotalBoxes = refrigerator.RefrigeratorDetails.Sum(m => m.CountOfBoxes);
                    refrigerator.TotalBalanceCardWeight = refrigerator.RefrigeratorDetails.Sum(m => m.BalanceCardWeight);
                    refrigerator.TotalEmptyBoxesWeight = refrigerator.RefrigeratorDetails.Sum(m => m.EmptyBoxesWeight * m.CountOfBoxes);

                    refrigerator.TotalWeightAfterDiscount_2Percent = refrigerator.RefrigeratorDetails.Sum(m => m.WeightAfterDiscount_2Percent);

                    // Calculate prices
                    refrigerator.TotalSalesPriceOfAll = refrigerator.RefrigeratorDetails.Sum(m => m.SalesPriceOfAll);


                    // Check if the buyer exists or add a new buyer if not
                    if (await GetOrCreateBuyerAsync(inDto, refrigerator))
                    {
                        if (await AddMoneytoFund(refrigerator.TotalSalesPriceOfAll))
                        {

                            _unitOfWork.repository<Refrigerator>().Add(refrigerator);
                            await _unitOfWork.Complete();

                            transactionScope.Complete(); // Commit transaction

                            return refrigerator.Id;
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

        private async Task<bool> GetOrCreateBuyerAsync(InputRefrigeratorDto inDto, Refrigerator inOut)
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
                    var refrigerator = await _unitOfWork.repository<Refrigerator>().GetByIdAsync(id);
                    if (refrigerator == null)
                    {
                        return false;
                    }
                    if (await AddMoneytoFund(-(refrigerator.TotalSalesPriceOfAll)))
                    {
                        _unitOfWork.repository<Refrigerator>().Delete(refrigerator);
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

        public async Task<List<RefrigeratorSimplifyDto>> GetAllAsync()
        {
            var refrigerators = await _unitOfWork.repository<Refrigerator>().GetAllAsync();
            return _mapper.Map<List<RefrigeratorSimplifyDto>>(refrigerators.ToList());
        }

        public async Task<RefrigeratorDto> GetByIdAsync(int id)
        {
            var refrigerator = (await _unitOfWork.repository<Refrigerator>().Get(m => m.Id == id)).FirstOrDefault();
            var res= _mapper.Map<RefrigeratorDto>(refrigerator);
            res.RefrigeratorDetailsDtos= _mapper.Map<List<RefrigeratorDetailsDto>>(refrigerator.RefrigeratorDetails.ToList());

            return res;
        }

        public async Task<List<RefrigeratorDto>> GetAllAsync_ForDesktop()
        {
            var refrigerators = await _unitOfWork.repository<Refrigerator>().GetAllAsync();
            var res = _mapper.Map<List<RefrigeratorDto>>(refrigerators.ToList());
            int index = 0;
            foreach(var single in res)
            {
                single.RefrigeratorDetailsDtos = _mapper.Map<List<RefrigeratorDetailsDto>>(refrigerators[index].RefrigeratorDetails.ToList());
                index++;
            }
            return res;
        }

    }

}
