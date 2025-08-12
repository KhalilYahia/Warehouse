
using Warehouse.Common;

namespace Warehouse.Services.DTO
{
    public class SearchInShipmentsDto
    {
        public DateTime? StartPeriod { get; set; }
        public DateTime? EndPeriod { get; set; }

        /// <summary>
        /// Идентификаторы OutboundDocument, представляющие номер отгрузки
        /// </summary>
        public List<int>? OutboundDocumentIds { get; set; }
        public List<int>? ClientIds { get; set; }
        public List<int>? ResourceIds { get; set; }
        public List<int>? UnitIds { get; set; }

    }
}
