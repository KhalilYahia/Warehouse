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
    public class CarsService : ICarsService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CarsService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        #region Buy section for add to repository
        public async Task<int> AddAsync(Input_CarDto inDto)
        {
            if (inDto == null) throw new ArgumentNullException(nameof(inDto));
            using (var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {

                    var inOut = _mapper.Map<Cars>(inDto);


                    // Check if the supplier exists or add a new supplier if not
                    inOut.FinancialEntitlement = await GetOrCreateSupplierAsync(inDto, inOut);

                    inOut.DriverId = inOut.FinancialEntitlement.SupplierId.Value;
                    // Calculate the cost
                    inOut.TotalPrice = (inOut.LoadsPerDay * inOut.PriceOfOne);


                    // Add the entity to the repository
                    _unitOfWork.repository<Cars>().Add(inOut);
                    await _unitOfWork.Complete();

                    // Update total funds after the entity has been added to the repository
                    await UpdateTotalFundsAsync(inOut.TotalPrice);

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

        private async Task<FinancialEntitlement> GetOrCreateSupplierAsync(Input_CarDto inDto, Cars inOut)
        {
            if (string.IsNullOrWhiteSpace(inDto.DriverName))
                throw new ArgumentException("Driver name cannot be null or empty");

            var supplierNameWithoutSpaces = Regex.Replace(inDto.DriverName, @"\s+", "");
            var supplier = (await _unitOfWork.repository<Supplier>().Get(m => m.SupplierNameWithoutSpaces == supplierNameWithoutSpaces)).FirstOrDefault();
            FinancialEntitlement res_;
            if (supplier != null)
            {
                inOut.DriverId = supplier.Id;
                inOut.DriverName = supplier.SupplierName;

                res_= await UpdateSupplierFinancialEntitlementsAsync(supplier, inDto, inOut);
               
                res_.SupplierId = inOut.DriverId;
            }
            else
            {
                supplier = new Supplier
                {
                    SupplierName = inOut.DriverName,
                    SupplierNameWithoutSpaces = supplierNameWithoutSpaces
                };
                res_ = await AddSupplierFinancialEntitlementsAsync(supplier, inDto, inOut);

                _unitOfWork.repository<Supplier>().Add(supplier);
                await _unitOfWork.Complete();

                res_.SupplierId = supplier.Id;
            }

            return res_;
        }

        private async Task<FinancialEntitlement> UpdateSupplierFinancialEntitlementsAsync(Supplier supplier, Input_CarDto inDto, Cars inOut)
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

            financialEntitlement.TotalAmount += (inDto.LoadsPerDay * inDto.PriceOfOne);
          
                financialEntitlement.Remainder = financialEntitlement.TotalAmount - financialEntitlement.TotalPayments;

                if (supplier.FinancialEntitlements == null)
                {
                    supplier.FinancialEntitlements = new List<FinancialEntitlement> { financialEntitlement };
                }
            return financialEntitlement;
        }

        private async Task<FinancialEntitlement> AddSupplierFinancialEntitlementsAsync(Supplier supplier, Input_CarDto inDto, Cars inOut)
        {
           
                supplier.FinancialEntitlements = new List<FinancialEntitlement>
                {
                    new FinancialEntitlement
                    {
                        Date = inDto.Date,
                        SupplierName = supplier.SupplierName,
                        TotalAmount = (inDto.LoadsPerDay * inDto.PriceOfOne),
                        TotalPayments=0,
                        Notes="",
                        Remainder=(inDto.LoadsPerDay * inDto.PriceOfOne)

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
                    var inOut = await _unitOfWork.repository<Cars>().GetByIdAsync(id);
            if (inOut == null)
            {
                return false; // Entity not found
            }

            // Retrieve the associated supplier
            var supplier = await _unitOfWork.repository<Supplier>().GetByIdAsync(inOut.DriverId);
            if (supplier != null && supplier.FinancialEntitlements != null)
            {
                // Update the supplier's financial entitlements
                var financialEntitlement = supplier.FinancialEntitlements.FirstOrDefault();
                if (financialEntitlement != null)
                {
                    financialEntitlement.TotalAmount -= (inOut.LoadsPerDay * inOut.PriceOfOne);

                    financialEntitlement.Remainder = financialEntitlement.TotalAmount - financialEntitlement.TotalPayments;

                    await UpdateTotalFundsAsync(-(inOut.LoadsPerDay * inOut.PriceOfOne));

                    _unitOfWork.repository<FinancialEntitlement>().UpdateAsync(financialEntitlement);

                }
            }

            // Remove the Repository_InOut entity
            _unitOfWork.repository<Cars>().Delete(inOut);
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



        public async Task<List<Cars_InSimplifyDto>> GetAllAsync()
        {
            var inOuts = await _unitOfWork.repository<Cars>().GetAllAsync();
            return _mapper.Map<List<Cars_InSimplifyDto>>(inOuts.ToList());
        }

        public async Task<Cars_InDetailsDto> GetByIdAsync(int id)
        {
            var inOut = (await _unitOfWork.repository<Cars>().Get(m=>m.Id==id)).FirstOrDefault();
            return _mapper.Map<Cars_InDetailsDto>(inOut);
        }

        public async Task<List<Cars_InDetailsDto>> GetAllAsync_ForDesktop()
        {
            var inOuts = await _unitOfWork.repository<Cars>().GetAllAsync();
            return _mapper.Map<List<Cars_InDetailsDto>>(inOuts.ToList());
        }


    }
}
