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
    public class ExternalEnvoicesService : IExternalEnvoicesService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ExternalEnvoicesService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        #region Add
        public async Task<int> AddAsync(InputExternalEnvoicesDto inDto)
        {
            if (inDto == null) throw new ArgumentNullException(nameof(inDto));

            var ExternalEnvoices = _mapper.Map<ExternalEnvoices>(inDto);
            // Check if the repository exists
            var repository = (await _unitOfWork.repository<RepositoryMaterials>().Get(m => m.Id == inDto.RepositoryMaterialId)).FirstOrDefault();
            if (repository == null)
                throw new KeyNotFoundException("RepositoryMaterial id not found");

            ExternalEnvoices.RepositoryMaterialId = repository.Id;
            ExternalEnvoices.MaterialName = repository.Name;
            ExternalEnvoices.CodeNumber = " ";

           
            // Calculate prices
            ExternalEnvoices.SalesPriceOfAll = ExternalEnvoices.Weight * inDto.SalesPriceOfUnit;


            // Check if the buyer exists or add a new buyer if not
            if (await GetOrCreateBuyerAsync(inDto, ExternalEnvoices))
            {
                if (await AddMoneytoFund(ExternalEnvoices.SalesPriceOfAll))
                {

                    _unitOfWork.repository<ExternalEnvoices>().Add(ExternalEnvoices);
                    await _unitOfWork.Complete();

                    return ExternalEnvoices.Id;
                }
            }
            return 0; // error
        }

        private async Task<bool> GetOrCreateBuyerAsync(InputExternalEnvoicesDto inDto, ExternalEnvoices inOut)
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
                    var ExternalEnvoices = await _unitOfWork.repository<ExternalEnvoices>().GetByIdAsync(id);
                    if (ExternalEnvoices == null)
                    {
                        return false;
                    }
                    if (await AddMoneytoFund(-(ExternalEnvoices.SalesPriceOfAll)))
                    {
                        _unitOfWork.repository<ExternalEnvoices>().Delete(ExternalEnvoices);
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

        public async Task<List<ExternalEnvoicesSimplifyDto>> GetAllAsync()
        {
            var ExternalEnvoicess = await _unitOfWork.repository<ExternalEnvoices>().GetAllAsync();
            return _mapper.Map<List<ExternalEnvoicesSimplifyDto>>(ExternalEnvoicess.ToList());
        }

        public async Task<ExternalEnvoicesDetailsDto> GetByIdAsync(int id)
        {
            var ExternalEnvoices = (await _unitOfWork.repository<ExternalEnvoices>().Get(m => m.Id == id)).FirstOrDefault();
            return _mapper.Map<ExternalEnvoicesDetailsDto>(ExternalEnvoices);
        }

        public async Task<List<ExternalEnvoicesDetailsDto>> GetAllAsync_Fordesktop()
        {
            var ExternalEnvoicess = await _unitOfWork.repository<ExternalEnvoices>().GetAllAsync();
            return _mapper.Map<List<ExternalEnvoicesDetailsDto>>(ExternalEnvoicess.ToList());
        }

    }

}
