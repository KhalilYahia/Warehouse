using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Warehouse.Common;
using Warehouse.Domain.Entities;

namespace Warehouse.Services.DTO
{
    public class AllActiveElementsDto
    {
        public List<ClientDto> Clients { get; set; }
        public List<ResourceDto> Resources { get; set; }
        public List<UnitDto> Units { get; set; }

    }

   
}
