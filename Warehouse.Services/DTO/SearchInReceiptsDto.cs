
using Warehouse.Common;

namespace Warehouse.Services.DTO
{
    public class SearchInReceiptsDto
    {
        public DateTime? StartPeriod { get; set; }
        public DateTime? EndPeriod { get; set; }
        /// <summary>
        /// Идентификаторы InboundDocument, представляющие номер поступления
        /// </summary>
        public List<int>? InboundDocumentIds { get; set; }
        public List<int>? ResourceIds { get; set; }
        public List<int>? UnitIds { get; set; }

    }
}
