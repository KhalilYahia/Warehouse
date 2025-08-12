using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Warehouse.Domain;
using Warehouse.Domain.Entities;
using Warehouse.Services.DTO;
using Warehouse.Services.Iservices;

namespace Warehouse.Services.services
{
    public class FinancialEntitlementService : IFinancialEntitlementService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public FinancialEntitlementService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<List<AllFinancialEntitlementDto>> GetAllFinancialEntitlementsAsync()
        {
            var res_db= (await _unitOfWork.repository<FinancialEntitlement>().GetAllAsync(includeProperties: "Repository_Ins")).OrderByDescending(m => m.Remainder).ToList();

            return _mapper.Map<List<FinancialEntitlement>, List<AllFinancialEntitlementDto>>(res_db);
        }



    }
}
