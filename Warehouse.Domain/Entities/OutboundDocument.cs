using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Warehouse.Domain.Entities
{
    public class OutboundDocument
    {
        public int Id { get; set; }
        public string Number { get; set; } = null!;
        public DateTime Date { get; set; }
        public int ClientId { get; set; }
        public bool IsSigned { get; set; }

        public Client Client { get; set; } = null!;
        public ICollection<OutboundItem> Items { get; set; } = new List<OutboundItem>();
    }
}
