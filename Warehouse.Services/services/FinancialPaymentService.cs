using Warehouse.Domain;
using Warehouse.Services.Iservices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Warehouse.Common;
using Warehouse.Domain.Entities;
using Warehouse.Services.DTO;
using Microsoft.AspNetCore.Hosting;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using System.Transactions;

namespace Warehouse.Services.services
{
    public class FinancialPaymentService : IFinancialPaymentService
    {

        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
      
        public FinancialPaymentService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
         
        }


        // Add a new FinancialPayment
        public async Task<int> AddFinancialPaymentAsync(FinancialPaymentDto newPayment)
        {
            var newPayment_ = _mapper.Map<FinancialPaymentDto, FinancialPayment>(newPayment);
            
            _unitOfWork.repository<FinancialPayment>().Add(newPayment_);
            await _unitOfWork.Complete();

            var entitlement = (await _unitOfWork.repository<FinancialEntitlement>().Get(m=>m.Id==newPayment.EntitlementId)).FirstOrDefault();
            if (entitlement != null)
            {
                entitlement.TotalPayments += newPayment.AmountPayment;
                entitlement.Remainder = entitlement.TotalAmount - entitlement.TotalPayments;
                _unitOfWork.repository<FinancialEntitlement>().Update(entitlement);
            }

            // Save changes to the database
            await _unitOfWork.Complete();

            // subtract AmountPayment from the total funds
            await UpdateTotalFundsAsync(newPayment.AmountPayment);

            return newPayment_.Id;

        }


        /// <summary>
        /// Updates the total funds by subtracting the specified payment amount.
        /// </summary>
        /// <param name="payment">The amount to subtract from the total funds.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains a boolean value indicating whether the update was successful.
        /// </returns>
        private async Task<bool> UpdateTotalFundsAsync(decimal payment)
        {
            // Find the existing entity by id
            var existingTotalFunds = (await _unitOfWork.repository<Resource>().GetAllAsync()).FirstOrDefault();
            if (existingTotalFunds == null)
            {
                return false; // Entity not found
            }

            // Update the properties
           
            existingTotalFunds.CurrentFund -= payment;
           

            // Save changes to the database
            await _unitOfWork.Complete();

            return true; // Update successful
        }


        /// <summary>
        /// Deletes a financial payment by its identifier.
        /// </summary>
        /// <param name="id">The identifier of the financial payment to be deleted.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. 
        /// The task result contains a boolean value indicating whether the deletion was successful.
        /// </returns>
        public async Task<bool> DeleteFinancialPaymentAsync(int id)
        {
            using (var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    var payments = (await _unitOfWork.repository<FinancialPayment>().Get(m => m.Id == id)).ToList();
                    if (!payments.Any())
                    {
                        return false; // Payment not found
                    }
                    var payment = payments.FirstOrDefault();

                    var entitlement = payment.FinancialEntitlement;

                    entitlement.TotalPayments -= payment.AmountPayment;
                    entitlement.Remainder = entitlement.TotalAmount - entitlement.TotalPayments;

                    if (await UpdateTotalFundsAsync(-payment.AmountPayment))
                    {
                        _unitOfWork.repository<FinancialEntitlement>().Update(entitlement);

                        _unitOfWork.repository<FinancialPayment>().Delete(payment);
                        await _unitOfWork.Complete();

                        transactionScope.Complete(); // Commit transaction

                        return true; // Deletion successful

                        
                    }
                    return false;
                }
                catch (Exception)
                {
                    throw;

                }
            }
        }

        // Get all FinancialPayments
        public async Task<List<FinancialPaymentDto>> GetAllFinancialPaymentsAsync(int EntitlementId)
        {
            var res=( await _unitOfWork.repository<FinancialPayment>().Get(m=>m.EntitlementId== EntitlementId)).ToList();

            return _mapper.Map<List<FinancialPayment>, List<FinancialPaymentDto>>(res);
        }

      

    }
}
