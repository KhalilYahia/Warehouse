using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Warehouse.Domain.Entities
{
    public class Balance
    {
        public int Id { get; set; }
        public int ResourceId { get; set; }
        public int UnitId { get; set; }
        public double Quantity { get; set; }

        // Tells EF Core to treat this as a concurrency token
        [Timestamp] 
        public byte[] RowVersion { get; set; }

        public Resource Resource { get; set; } = null!;
        public Unit Unit { get; set; } = null!;
    }
}
