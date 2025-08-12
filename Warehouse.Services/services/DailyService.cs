using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Transactions;
using Warehouse.Data;
using Warehouse.Domain;
using Warehouse.Domain.Entities;
using Warehouse.Services.DTO;
using Warehouse.Services.Iservices;

namespace Warehouse.Services.services
{
    public class DailyService : IDailyService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public DailyService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        #region Buy section for add to repository
        public async Task<int> AddAsync(InputDailyDto inDto)
        {
            if (inDto == null) throw new ArgumentNullException(nameof(inDto));
            using (var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    var inOut = _mapper.Map<Daily>(inDto);

                    // Check if the repository exists
                    var repository = (await _unitOfWork.repository<RepositoryMaterials>().Get(m => m.Id == inDto.RepositoryMaterialId)).FirstOrDefault();
                    if (repository == null)
                        throw new KeyNotFoundException("RepositoryMaterial id not found");

                    inOut.RepositoryMaterialId = repository.Id;
                    inOut.MaterialName = repository.Name;

                    // Check if the supplier exists or add a new supplier if not
                    inOut.FinancialEntitlement = await GetOrCreateSupplierAsync(inDto, inOut);

                    inOut.SupplierOfFarmsFinancialEntitlement = await GetOrCreateSupplierOfFarmrsAsync(inDto, inOut);

                    inOut.FarmerId = inOut.FinancialEntitlement.SupplierId.Value;

                   

                    inOut.WeightAfterDiscount_2Percent = (inDto.BalanceCardWeight - (inDto.TotalBoxes * inDto.EmptyBoxesWeight)) * ((decimal)0.98);
                    // Calculate prices
                    inOut.BuyPriceOfAll = inOut.WeightAfterDiscount_2Percent * inDto.BuyPriceOfUnit;

                    inOut.CuttingCostOfAll = inOut.CuttingCostOfUnit * inOut.WeightAfterDiscount_2Percent;

                    inOut.WaxingCostOfAll = inOut.WeightAfterDiscount_2Percent * inOut.WaxingCostOfUnit;

                    var waxing_factory_name = (await _unitOfWork.repository<Supplier>().Get(m => m.SupplierName == "الشماعة")).First();
                    inOut.WaxingFactory_As_dealer = waxing_factory_name;
                    inOut.WaxingFactory_As_dealerId = waxing_factory_name.Id;

                    if (inOut.WaxingFactory_As_dealer.FinancialEntitlements != null && (inOut.WaxingFactory_As_dealer.FinancialEntitlements.Any()))
                    {
                        inOut.WaxingFactory_As_dealer.FinancialEntitlements.First().TotalAmount += inOut.WaxingCostOfAll;
                        inOut.WaxingFactory_As_dealer.FinancialEntitlements.First().Remainder = inOut.WaxingFactory_As_dealer.FinancialEntitlements.First().TotalAmount - inOut.WaxingFactory_As_dealer.FinancialEntitlements.First().TotalPayments;
                        inOut.WaxingFactoryEntitlementId = inOut.WaxingFactory_As_dealer.FinancialEntitlements.First().Id;
                    }
                    else
                    {
                        inOut.WaxingFactory_As_dealer.FinancialEntitlements =  new List<FinancialEntitlement>
                            {
                                new FinancialEntitlement
                                {
                                    Date = inDto.Date,
                                    SupplierName = inOut.WaxingFactory_As_dealer.SupplierName,
                                    TotalAmount = inOut.WaxingCostOfAll,
                                    TotalPayments =0,
                                    Notes="",
                                    Remainder=inOut.WaxingCostOfAll

                                }
                            };
                        inOut.WaxingFactoryFinancialEntitlement = inOut.WaxingFactory_As_dealer.FinancialEntitlements.First();
                    }

                    // Add the entity to the repository
                    _unitOfWork.repository<Daily>().Add(inOut);
                    await _unitOfWork.Complete();
                  
                    // Update total funds after the entity has been added to the repository
                    await UpdateTotalFundsAsync(inOut.BuyPriceOfAll + inOut.CuttingCostOfAll + inOut.WaxingCostOfAll);

                    transactionScope.Complete(); // Commit transaction
                    return inOut.Id;
                }
                catch (Exception)
                {
                    // If an exception is thrown, it will rollback automatically
                    throw;
                }
            }
        }

        private async Task<FinancialEntitlement> GetOrCreateSupplierAsync(InputDailyDto inDto, Daily inOut)
        {
            if (string.IsNullOrWhiteSpace(inDto.FarmerName))
                throw new ArgumentException("Farmer name cannot be null or empty");

            var supplierNameWithoutSpaces = Regex.Replace(inDto.FarmerName, @"\s+", "");
            var supplier = (await _unitOfWork.repository<Supplier>().Get(m => m.SupplierNameWithoutSpaces == supplierNameWithoutSpaces)).FirstOrDefault();
            FinancialEntitlement res_;
            if (supplier != null)
            {
                inOut.FarmerId = supplier.Id;
                inOut.FarmerName = supplier.SupplierName;

                res_= await UpdateSupplierFinancialEntitlementsAsync(supplier, inDto, inOut);
                res_.SupplierId = inOut.FarmerId;
            }
            else
            {
                supplier = new Supplier
                {
                    SupplierName = inOut.FarmerName,
                    SupplierNameWithoutSpaces = supplierNameWithoutSpaces
                };
                res_ = await AddSupplierFinancialEntitlementsAsync(supplier, inDto, inOut);

                _unitOfWork.repository<Supplier>().Add(supplier);
                await _unitOfWork.Complete();

                res_.SupplierId=supplier.Id;
            }

            return res_;
        }
        private async Task<FinancialEntitlement> GetOrCreateSupplierOfFarmrsAsync(InputDailyDto inDto, Daily inOut)
        {
            if (string.IsNullOrWhiteSpace(inDto.Supplier))
                throw new ArgumentException("Supplier name cannot be null or empty");

            var supplierNameWithoutSpaces = Regex.Replace(inDto.Supplier, @"\s+", "");
            var supplier = (await _unitOfWork.repository<SupplierOfFarms>().Get(m => m.NameWithoutSpaces == supplierNameWithoutSpaces)).FirstOrDefault();
            FinancialEntitlement res_;
            if (supplier != null)
            {
                inOut.SupplierOfFarmsId = supplier.Id;
                inOut.Supplier = supplier.Name;
                res_ = await UpdateSupplierForFarms_FinancialEntitlementsAsync(supplier, inDto, inOut);
                res_.SupplierOfFarmId = supplier.Id;
            }
            else
            {
                supplier = new SupplierOfFarms
                {
                    Name = inOut.Supplier,
                    NameWithoutSpaces = supplierNameWithoutSpaces
                };
                res_ = await AddSupplierForFarms_FinancialEntitlementsAsync(supplier, inDto, inOut);

                _unitOfWork.repository<SupplierOfFarms>().Add(supplier);
                await _unitOfWork.Complete();

                inOut.SupplierOfFarmsId = supplier.Id;
                inOut.Supplier = supplier.Name;


            }
            return res_;

        }

        private async Task<FinancialEntitlement> UpdateSupplierFinancialEntitlementsAsync(Supplier supplier, InputDailyDto inDto, Daily inOut)
        {
           
                var financialEntitlement = supplier.FinancialEntitlements?.FirstOrDefault() ?? new FinancialEntitlement
                {
                    SupplierName = supplier.SupplierName,
                    Date = inDto.Date,
                    Remainder=0,
                    TotalAmount = 0,
                    TotalPayments = 0,
                    Notes=""
                };

                financialEntitlement.TotalAmount += (inDto.BalanceCardWeight - (inDto.TotalBoxes*inDto.EmptyBoxesWeight)) * inDto.BuyPriceOfUnit*((decimal)0.98);
          
                financialEntitlement.Remainder = financialEntitlement.TotalAmount - financialEntitlement.TotalPayments;

                if (supplier.FinancialEntitlements == null)
                {
                    supplier.FinancialEntitlements = new List<FinancialEntitlement> { financialEntitlement };
                }
            return financialEntitlement;
        }
        private async Task<FinancialEntitlement> UpdateSupplierForFarms_FinancialEntitlementsAsync(SupplierOfFarms supplier, InputDailyDto inDto, Daily inOut)
        {

            var financialEntitlement = supplier.FinancialEntitlements?.FirstOrDefault() ?? new FinancialEntitlement
            {
                SupplierName = supplier.Name,
                Date = inDto.Date,
                Remainder = 0,
                TotalAmount = 0,
                TotalPayments = 0,
                Notes = ""
                 
            };

            financialEntitlement.TotalAmount += (inDto.BalanceCardWeight - (inDto.TotalBoxes * inDto.EmptyBoxesWeight)) * inDto.CuttingCostOfUnit * ((decimal)0.98);

            financialEntitlement.Remainder = financialEntitlement.TotalAmount - financialEntitlement.TotalPayments;

            if (supplier.FinancialEntitlements == null)
            {
                supplier.FinancialEntitlements = new List<FinancialEntitlement> { financialEntitlement };
            }
            return financialEntitlement;
        }

        private async Task<FinancialEntitlement> AddSupplierFinancialEntitlementsAsync(Supplier supplier, InputDailyDto inDto, Daily inOut)
        {
           
                supplier.FinancialEntitlements = new List<FinancialEntitlement>
                {
                    new FinancialEntitlement
                    {
                        Date = inDto.Date,
                        SupplierName = supplier.SupplierName,
                        TotalAmount = (inDto.BalanceCardWeight - (inDto.TotalBoxes*inDto.EmptyBoxesWeight)) * inDto.BuyPriceOfUnit*((decimal)0.98),
                        TotalPayments =0,
                        Notes="",
                        Remainder=(inDto.BalanceCardWeight - (inDto.TotalBoxes*inDto.EmptyBoxesWeight)) * inDto.BuyPriceOfUnit*((decimal)0.98)

        }
                };
            return supplier.FinancialEntitlements.FirstOrDefault();
        }

        private async Task<FinancialEntitlement> AddSupplierForFarms_FinancialEntitlementsAsync(SupplierOfFarms supplier, InputDailyDto inDto, Daily inOut)
        {

            supplier.FinancialEntitlements = new List<FinancialEntitlement>
                {
                    new FinancialEntitlement
                    {
                        Date = inDto.Date,
                        SupplierName = supplier.Name,
                        TotalAmount = (inDto.BalanceCardWeight - (inDto.TotalBoxes*inDto.EmptyBoxesWeight)) * inDto.CuttingCostOfUnit*((decimal)0.98),
                        TotalPayments =0,
                        Notes="",
                        Remainder=(inDto.BalanceCardWeight - (inDto.TotalBoxes*inDto.EmptyBoxesWeight)) * inDto.CuttingCostOfUnit*((decimal)0.98)

        }
                };
            return supplier.FinancialEntitlements.FirstOrDefault();
        }


        /// <summary>
        /// Updates the total funds by adding the provided amount to the TotalOut property.
        /// </summary>
        /// <param name="TotalOut">The amount to be added to the TotalOut property.</param>
        /// <returns>
        /// Returns true if the update is successful and the TotalOut property is updated.
        /// Returns false if the TotalFunds entity is not found in the database.
        /// </returns>
        private async Task<bool> UpdateTotalFundsAsync(decimal TotalOut)
        {
            // Find the existing entity by id
            var existingTotalFunds = (await _unitOfWork.repository<Resource>().GetAllAsync()).FirstOrDefault();
            if (existingTotalFunds == null)
            {
                return false; // Entity not found
            }

            // Update the properties
            existingTotalFunds.TotalOut += TotalOut;
            existingTotalFunds.Profits = existingTotalFunds.TotalIn- existingTotalFunds.TotalOut;

            _unitOfWork.repository<Resource>().UpdateAsync(existingTotalFunds);
            // Save changes to the database
            await _unitOfWork.Complete();

            return true; // Update successful
        }

        #endregion

        //// وصلت الى هنا بتاريخ 4/8/2024

        /// <summary>
        /// هذا التابع لم أقم بفحصه بعد
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<bool> DeleteAsync(int id)
        {
            using (var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                { 
                // Retrieve the Repository_InOut entity by ID
                var inOut = await _unitOfWork.repository<Daily>().GetByIdAsync(id);
            if (inOut == null)
            {
                return false; // Entity not found
            }

            // Retrieve the associated supplier
            var supplier = await _unitOfWork.repository<Supplier>().GetByIdAsync(inOut.FarmerId);
            var supplierOfFarms = await _unitOfWork.repository<SupplierOfFarms>().GetByIdAsync(inOut.SupplierOfFarmsId);
            
                    Supplier WaxingFactory_As_dealer = null;
            if (inOut.WaxingFactory_As_dealerId.HasValue)
                    WaxingFactory_As_dealer = await _unitOfWork.repository<Supplier>().GetByIdAsync(inOut.WaxingFactory_As_dealerId.Value);
            
            if (supplier != null && supplier.FinancialEntitlements != null)
            {
                // Update the supplier's financial entitlements
                var financialEntitlement = supplier.FinancialEntitlements.FirstOrDefault();
                if (financialEntitlement != null)
                {
                    financialEntitlement.TotalAmount -= (inOut.BuyPriceOfAll);

                    financialEntitlement.Remainder = financialEntitlement.TotalAmount - financialEntitlement.TotalPayments;

                    await UpdateTotalFundsAsync(-(inOut.BuyPriceOfAll));

                    _unitOfWork.repository<FinancialEntitlement>().UpdateAsync(financialEntitlement);

                }
            }

            if (supplierOfFarms != null && supplierOfFarms.FinancialEntitlements != null)
            {
                // Update the supplier's financial entitlements
                var financialEntitlement = supplierOfFarms.FinancialEntitlements.FirstOrDefault();
                if (financialEntitlement != null)
                {
                    financialEntitlement.TotalAmount -= inOut.CuttingCostOfAll;

                    financialEntitlement.Remainder = financialEntitlement.TotalAmount - financialEntitlement.TotalPayments;

                    await UpdateTotalFundsAsync(-(inOut.CuttingCostOfAll));

                    _unitOfWork.repository<FinancialEntitlement>().UpdateAsync(financialEntitlement);

                }
            }

            if (WaxingFactory_As_dealer != null && WaxingFactory_As_dealer.FinancialEntitlements != null)
            {
                // Update the supplier's financial entitlements
                var financialEntitlement = WaxingFactory_As_dealer.FinancialEntitlements.FirstOrDefault();
                if (financialEntitlement != null)
                {
                    financialEntitlement.TotalAmount -= inOut.WaxingCostOfAll;

                    financialEntitlement.Remainder = financialEntitlement.TotalAmount - financialEntitlement.TotalPayments;

                    await UpdateTotalFundsAsync(-(inOut.WaxingCostOfAll));

                    _unitOfWork.repository<FinancialEntitlement>().UpdateAsync(financialEntitlement);

                }
            }

                    // Remove the Repository_InOut entity
                    _unitOfWork.repository<Daily>().Delete(inOut);
                await _unitOfWork.Complete();
                transactionScope.Complete(); // Commit transaction
                return true; // Successfully deleted
       
             }

            catch (Exception)
            {
                // If an exception is thrown, it will rollback automatically
                throw;
            }
        }
          
        }

        public async Task<int> UpdateAsync(InputDailyDto inDto)
        {
            using (var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                int res_ = 0;
                try
                {
                    if(await DeleteAsync(inDto.Id))
                    {
                        var indto_ = _mapper.Map<InputDailyDto>(inDto);
                        indto_.Id = 0;
                        res_ = await AddAsync(indto_);
                        
                        transactionScope.Complete(); // Commit transaction
                        return res_;
                    }
                    return 0;
                   
                }
                catch(Exception)
                {
                    throw;
                }
            }
        }
        public async Task<List<DailySimplifyDto>> GetAllAsync()
        {
            var inOuts = await _unitOfWork.repository<Daily>().GetAllAsync();
            return _mapper.Map<List<DailySimplifyDto>>(inOuts.ToList());
        }

        public async Task<DailyDetailsDto> GetByIdAsync(int id)
        {
            var inOut = (await _unitOfWork.repository<Daily>().Get(m=>m.Id==id)).FirstOrDefault();
            return _mapper.Map<DailyDetailsDto>(inOut);
        }

        public async Task<List<DailyDetailsDto>> GetAllAsync_Fordesktop()
        {
            var inOuts = await _unitOfWork.repository<Daily>().GetAllAsync();
            return _mapper.Map<List<DailyDetailsDto>>(inOuts.ToList());
        }


    }
}
