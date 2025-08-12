using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Warehouse.Common;
using Warehouse.Domain.Entities;

namespace Warehouse.Services.DTO
{
    public class UnitDto
    {
        /// <summary>
        /// Чтобы добавить новое значение, установите 0
        /// </summary>
        public int Id { get; set; }
        public string Name { get; set; }
        /// <summary>
        /// ВРаботе = 0, ВАрхиве = 1
        /// </summary>
        public STATUS Status { get; set; }

    }
}
