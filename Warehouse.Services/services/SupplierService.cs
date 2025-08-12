using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Warehouse.Domain;
using Warehouse.Domain.Entities;
using Warehouse.Services.DTO;
using Warehouse.Services.Iservices;

namespace Warehouse.Services.services
{
    public class SupplierService : ISupplierService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public SupplierService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<int> AddAsync(SupplierDto supplierDto)
        {
            var supplier = _mapper.Map<Supplier>(supplierDto);
            supplier.SupplierNameWithoutSpaces = Regex.Replace(supplier.SupplierName, @"\s+", "");// remove spaces from supplier name

            _unitOfWork.repository<Supplier>().Add(supplier);
            await _unitOfWork.Complete();
            return supplier.Id;
        }

        public async Task<bool> UpdateAsync(SupplierDto supplierDto)
        {
           
            var supplier = _mapper.Map<Supplier>(supplierDto);
            _unitOfWork.repository<Supplier>().UpdateAsync(supplier);
            await _unitOfWork.Complete();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var supplier = await _unitOfWork.repository<Supplier>().GetByIdAsync(id);
            if (supplier == null)
            {
                throw new KeyNotFoundException("Supplier not found");
            }
            if (supplier.Cars.Any() || supplier.Dailies.Any() ||
               supplier.FinancialEntitlements.Any() || supplier.Fuels.Any()
               || supplier.Repository_InOuts.Any())
            {
                return false;
            }

            _unitOfWork.repository<Supplier>().DeleteAsync(supplier);
            await _unitOfWork.Complete();
            return true;
        }


        public async Task<List<SupplierDto>> GetAllAsync()
        {
            var suppliers = await _unitOfWork.repository<Supplier>().GetAllAsync();
            return _mapper.Map<List<SupplierDto>>(suppliers.ToList());
        }

        public async Task<List<SupplierDto>> GetAllSupplierOfFarmsAsync()
        {
            var suppliers = await _unitOfWork.repository<SupplierOfFarms>().GetAllAsync();
            return _mapper.Map<List<SupplierDto>>(suppliers.ToList());
        }

        public async Task<SupplierDto> GetByIdAsync(int id)
        {
            var supplier = await _unitOfWork.repository<Supplier>().GetByIdAsync(id);
            return _mapper.Map<SupplierDto>(supplier);
        }

      
    }

}
