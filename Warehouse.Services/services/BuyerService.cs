using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Warehouse.Domain.Entities;
using Warehouse.Domain;
using Warehouse.Services.Iservices;
using Warehouse.Services.DTO;

namespace Warehouse.Services.services
{
    public class BuyerService : IBuyerService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public BuyerService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<int> AddAsync(BuyerDto inDto)
        {
            if (inDto == null) throw new ArgumentNullException(nameof(inDto));

            var buyer = _mapper.Map<Buyers>(inDto);
         

            _unitOfWork.repository<Buyers>().Add(buyer);
            await _unitOfWork.Complete();

            return buyer.Id;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var buyer = await _unitOfWork.repository<Buyers>().GetByIdAsync(id);
            if (buyer == null)
            {
                return false;
            }
            if (buyer.ExternalEnvoices.Any() || buyer.CoolingRooms.Any() ||
                buyer.OtherSales.Any() || buyer.Refrigerators.Any())
            {
                return false;
            }

            _unitOfWork.repository<Buyers>().Delete(buyer);
            await _unitOfWork.Complete();

            return true;
        }

        public async Task<List<BuyerDto>> GetAllAsync()
        {
            var buyers = await _unitOfWork.repository<Buyers>().GetAllAsync();
            return _mapper.Map<List<BuyerDto>>(buyers.ToList());
        }

        public async Task<BuyerDto> GetByIdAsync(int id)
        {
            var buyer = (await _unitOfWork.repository<Buyers>().Get(m => m.Id == id)).FirstOrDefault();
            return _mapper.Map<BuyerDto>(buyer);
        }

        public async Task<bool> UpdateAsync(BuyerDto inDto)
        {
            if (inDto == null) throw new ArgumentNullException(nameof(inDto));

            var buyer = await _unitOfWork.repository<Buyers>().GetByIdAsync(inDto.Id);
            if (buyer == null)
            {
                return false; // Or handle not found case
            }

            _mapper.Map(inDto, buyer);

            _unitOfWork.repository<Buyers>().UpdateAsync(buyer);
            await _unitOfWork.Complete();

            return true;
        }
    }
}
