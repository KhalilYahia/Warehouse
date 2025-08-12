
using Warehouse.Common;

namespace Warehouse.Services.DTO
{
    public class ClientDto
    {
        /// <summary>
        /// Чтобы добавить новое значение, установите 0
        /// </summary>
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; } 
        public STATUS Status { get; set; }

    }
}
