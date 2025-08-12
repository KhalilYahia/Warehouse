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
    public class RepositoryInOutService : IRepositoryInOutServices
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public RepositoryInOutService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        #region Buy section for add to repository
        public async Task<int> AddAsync(Input_RepositoryInDto inDto)
        {
            if (inDto == null) throw new ArgumentNullException(nameof(inDto));
            var category_ = (await _unitOfWork.repository<Category>().Get(m => m.Id == inDto.CategoryId)).FirstOrDefault();
            if (category_ == null)
                throw new KeyNotFoundException("Category id not found");
            using (var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    var inOut = _mapper.Map<Repository_InOut>(inDto);

            // Check if the repository exists
            var repository = await GetOrCreatMaterialAsync(inDto);
            if (repository == null)
                throw new KeyNotFoundException("RepositoryMaterial id not found");

            inOut.RepositoryMaterialId = repository.Id;
            inOut.Name = repository.Name;

            // Check if the supplier exists or add a new supplier if not
            inOut.FinancialEntitlement = await GetOrCreateSupplierAsync(inDto, inOut);

            inOut.SupplierId = inOut.FinancialEntitlement.SupplierId.Value;
            // Calculate prices
            inOut.BuyPriceOfAll = inOut.Amount * inOut.BuyPriceOfUnit;
            inOut.SoldPriceOfUnit = 0;
            inOut.SoldPriceOfAll = 0;
            inOut.Direction = Common.Utils.DirectionType.In;


            // Add the entity to the repository
            _unitOfWork.repository<Repository_InOut>().Add(inOut);
            await _unitOfWork.Complete();

            // Update total funds after the entity has been added to the repository
            await UpdateTotalFundsAsync(inOut.BuyPriceOfAll);

                    transactionScope.Complete(); // Commit transaction

                    return inOut.Id;
                }
                catch (Exception)
                {
                    // If anything fails, the transaction will be rolled back
                    throw;
                }
            }
        }

        private async Task<RepositoryMaterials> GetOrCreatMaterialAsync(Input_RepositoryInDto inDto)
        {
            if (string.IsNullOrWhiteSpace(inDto.RepositoryMaterialName))
                throw new ArgumentException("RepositoryMaterial name cannot be null or empty");

            var RepositoryMaterialNameWithoutSpaces = Regex.Replace(inDto.RepositoryMaterialName, @"\s+", "");
            var RepositoryMaterial_ = (await _unitOfWork.repository<RepositoryMaterials>().Get(m =>m.Name.Replace(" ", "") == RepositoryMaterialNameWithoutSpaces)).FirstOrDefault();
            RepositoryMaterials res_;
            if (RepositoryMaterial_ != null)
            {
                res_ = RepositoryMaterial_;
            }
            else
            {
                res_ = new RepositoryMaterials
                {
                   DefaultPrice=0,
                   Name= inDto.RepositoryMaterialName,
                   CategoryId=inDto.CategoryId,
                   DefaultSoldPrice=0,
                   Sort=10
                };

                _unitOfWork.repository<RepositoryMaterials>().Add(res_);
                await _unitOfWork.Complete();
            }

            return res_;
        }

        private async Task<FinancialEntitlement> GetOrCreateSupplierAsync(Input_RepositoryInDto inDto, Repository_InOut inOut)
        {
            if (string.IsNullOrWhiteSpace(inDto.SupplierName))
                throw new ArgumentException("Supplier name cannot be null or empty");

            var supplierNameWithoutSpaces = Regex.Replace(inDto.SupplierName, @"\s+", "");
            var supplier = (await _unitOfWork.repository<Supplier>().Get(m => m.SupplierNameWithoutSpaces == supplierNameWithoutSpaces)).FirstOrDefault();
            FinancialEntitlement res_;
            if (supplier != null)
            {
                inOut.SupplierId = supplier.Id;
                inOut.SupplierName = supplier.SupplierName;

                res_= await UpdateSupplierFinancialEntitlementsAsync(supplier, inDto, inOut);
                res_.SupplierId = inOut.SupplierId;
            }
            else
            {
                supplier = new Supplier
                {
                    SupplierName = inOut.SupplierName,
                    SupplierNameWithoutSpaces = supplierNameWithoutSpaces
                };
                res_ = await AddSupplierFinancialEntitlementsAsync(supplier, inDto, inOut);

                _unitOfWork.repository<Supplier>().Add(supplier);
                await _unitOfWork.Complete();

                res_.SupplierId = supplier.Id;
            }

            return res_;
        }

        private async Task<FinancialEntitlement> UpdateSupplierFinancialEntitlementsAsync(Supplier supplier, Input_RepositoryInDto inDto, Repository_InOut inOut)
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

                financialEntitlement.TotalAmount += inDto.Amount * inDto.BuyPriceOfUnit;
          
                financialEntitlement.Remainder = financialEntitlement.TotalAmount - financialEntitlement.TotalPayments;

                if (supplier.FinancialEntitlements == null)
                {
                    supplier.FinancialEntitlements = new List<FinancialEntitlement> { financialEntitlement };
                }
            return financialEntitlement;
        }

        private async Task<FinancialEntitlement> AddSupplierFinancialEntitlementsAsync(Supplier supplier, Input_RepositoryInDto inDto, Repository_InOut inOut)
        {
           
                supplier.FinancialEntitlements = new List<FinancialEntitlement>
                {
                    new FinancialEntitlement
                    {
                        Date = inDto.Date,
                        SupplierName = supplier.SupplierName,
                        TotalAmount = inDto.Amount * inDto.BuyPriceOfUnit,
                        TotalPayments=0,
                        Notes="",
                        Remainder=inDto.Amount * inDto.BuyPriceOfUnit

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
                    var inOut = await _unitOfWork.repository<Repository_InOut>().GetByIdAsync(id);
                    if (inOut == null)
                    {
                        return false; // Entity not found
                    }

                    // Retrieve the associated supplier
                    var supplier = await _unitOfWork.repository<Supplier>().GetByIdAsync(inOut.SupplierId);
                    if (supplier != null && supplier.FinancialEntitlements != null)
                    {
                        // Update the supplier's financial entitlements
                        var financialEntitlement = supplier.FinancialEntitlements.FirstOrDefault();
                        if (financialEntitlement != null)
                        {
                            financialEntitlement.TotalAmount -= inOut.Amount * inOut.BuyPriceOfUnit;

                            financialEntitlement.Remainder = financialEntitlement.TotalAmount - financialEntitlement.TotalPayments;

                            await UpdateTotalFundsAsync(-(inOut.Amount * inOut.BuyPriceOfUnit));

                            _unitOfWork.repository<FinancialEntitlement>().UpdateAsync(financialEntitlement);

                        }
                    }

                    // Remove the Repository_InOut entity
                    _unitOfWork.repository<Repository_InOut>().Delete(inOut);
                    await _unitOfWork.Complete();

                    transactionScope.Complete(); // Commit transaction

                    return true; // Successfully deleted
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }


        public async Task<List<Ripository_InSimplifyDto>> GetAllAsync()
        {
            var inOuts = await _unitOfWork.repository<Repository_InOut>().GetAllAsync();
            return _mapper.Map<List<Ripository_InSimplifyDto>>(inOuts.ToList());
        }

        public async Task<Ripository_InDetailsDto> GetByIdAsync(int id)
        {
            var inOut = (await _unitOfWork.repository<Repository_InOut>().Get(m=>m.Id==id)).FirstOrDefault();
            return _mapper.Map<Ripository_InDetailsDto>(inOut);
        }

        #region desktop

        public async Task<List<Ripository_InDetailsDto>> GetAll_Desktop()
        {
            var inOut = (await _unitOfWork.repository<Repository_InOut>().GetAllAsync());
            return _mapper.Map<List<Ripository_InDetailsDto>>(inOut);
        }

        #endregion


    }
}
