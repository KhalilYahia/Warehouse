using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Warehouse.Common;

namespace Warehouse.Domain.Entities
{
    public class Client
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Address { get; set; } = null!;
        public STATUS Status { get; set; } = STATUS.InWork;

        public ICollection<OutboundDocument> OutboundDocuments { get; set; } = new List<OutboundDocument>();

    }
}
