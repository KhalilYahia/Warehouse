using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Warehouse.Common;
using Warehouse.Domain.Entities;
using Warehouse.Services.DTO;

namespace Warehouse.Services.Iservices
{
    public interface IBalanceService
    {
        Task<List<BalanceDto>> Search(SearchInBalanceDto dto);
        Task<AllActiveElementsDto> GetAllActiveElements();

        Task<List<BalanceDto>> GetAllActivatedInBalance();
    }

}
