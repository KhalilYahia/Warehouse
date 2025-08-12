
using Warehouse.Common;

namespace Warehouse.Services.DTO
{
    public class ShipmentDto
    {
        /// <summary>
        /// Чтобы добавить новое значение, установите 0
        /// </summary>
        public int Id { get; set; } // this from OutboundDocument
        public string Number { get; set; } 
        public DateTime Date { get; set; }

        public int ClientId { get; set; }
        public string ClientName { get; set; }

        public bool IsSigned { get; set; }

        public List<OutboundItemDto> Goods { get; set; }

    }

    public class OutboundItemDto
    {
        public int Id { get; set; }
        public int ResourceId { get; set; }
        public string ResourceName { get; set; } = null!;

        public int UnitId { get; set; }
        public string UnitName { get; set; } = null!;

        public double Quantity { get; set; }

    }
}
