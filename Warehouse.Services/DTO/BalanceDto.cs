
using System.ComponentModel.DataAnnotations;
using Warehouse.Common;
using Warehouse.Domain.Entities;

namespace Warehouse.Services.DTO
{
    public class BalanceDto
    {
        public int Id { get; set; }
        public int ResourceId { get; set; }
        public string ResourceName { get; set; }

        public int UnitId { get; set; }
        public string UnitName { get; set; }

        public double Quantity { get; set; }


    }

}
