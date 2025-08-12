using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Warehouse.Domain.Entities
{
    public class OutboundItem
    {
        public int Id { get; set; }
        public int OutboundDocumentId { get; set; }
        public int ResourceId { get; set; }
        public int UnitId { get; set; }
        public double Quantity { get; set; }

        public OutboundDocument OutboundDocument { get; set; } = null!;
        public Resource Resource { get; set; } = null!;
        public Unit Unit { get; set; } = null!;
    }
}
