using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Warehouse.Common;

namespace Warehouse.Domain.Entities
{
    public class Resource
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public STATUS Status { get; set; } = STATUS.InWork;

        public ICollection<Balance> Balances { get; set; } = new List<Balance>();
        public ICollection<InboundItem> InboundItems { get; set; } = new List<InboundItem>();
        public ICollection<OutboundItem> OutboundItems { get; set; } = new List<OutboundItem>();

    }
}
