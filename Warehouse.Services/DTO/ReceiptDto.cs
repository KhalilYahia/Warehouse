
using Warehouse.Common;

namespace Warehouse.Services.DTO
{
    public class ReceiptDto
    {
        /// <summary>
        /// Чтобы добавить новое значение, установите 0
        /// </summary>
        public int Id { get; set; } // this from InboundDocument
        public string Number { get; set; } 
        public DateTime Date { get; set; }

        public List<InboundItemDto> Goods { get; set; }

    }

    public class InboundItemDto
    {
        public int Id { get; set; }
        public int ResourceId { get; set; }
        public string ResourceName { get; set; } = null!;

        public int UnitId { get; set; }
        public string UnitName { get; set; } = null!;

        public double Quantity { get; set; }
    }
}
