using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Warehouse.Common;
using Warehouse.Domain.Entities;

namespace Warehouse.Services.DTO
{
    public class FilterReceiptRequiresDto
    {
        public List<InboundDocDto> DocumentsNumber { get; set; }
        public List<ResourceDto> Resources { get; set; }
        public List<UnitDto> Units { get; set; }
        public List<ClientDto> Clients { get; set; }

    }
    public class InboundDocDto
    {
        public int Id { get; set; }
        public string Number { get; set; }

    }
}
