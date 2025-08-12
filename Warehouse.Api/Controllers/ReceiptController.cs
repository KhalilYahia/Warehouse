using Warehouse.Common;
using Warehouse.Services.DTO;
using Warehouse.Services.Iservices;
using Microsoft.AspNetCore.Mvc;
using Warehouse.Services.services;


namespace Warehouse.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public class ReceiptController : ControllerBase
    {
        private readonly IReceiptService _receiptService;

        public ReceiptController(IReceiptService receiptService)
        {
            _receiptService = receiptService;
        }

        /// <summary>
        /// Поиск приходных документов по заданным параметрам.
        /// </summary>
        /// <param name="dto">Параметры поиска (SearchInReceiptsDto).</param>
        /// <returns>Список найденных приходных документов (ReceiptDto).</returns>
        [HttpPost("Search")]
        public async Task<ActionResult<List<ReceiptDto>>> Search([FromBody] SearchInReceiptsDto dto)
        {
            var result = await _receiptService.Search(dto);
            return Ok(result);
        }

        /// <summary>
        /// Получить приходной документ по идентификатору.
        /// </summary>
        /// <param name="InboundDocument_Id">Идентификатор приходного документа.</param>
        /// <returns>Объект ReceiptDto, если документ найден; иначе 404 с сообщением.</returns>
        [HttpGet("GetById")]
        public async Task<ActionResult<ReceiptDto>> GetById(int InboundDocument_Id)
        {
            var receipt = await _receiptService.GetByIdAsync(InboundDocument_Id);
            if (receipt == null)
                return NotFound($"Приходной документ с идентификатором {InboundDocument_Id} не найден.");
            return Ok(receipt);
        }

        /// <summary>
        /// Создать новый приходной документ.
        /// </summary>
        /// <param name="dto">Данные приходного документа для создания.</param>
        /// <returns>Идентификатор созданного документа.</returns>
        [HttpPost("Create")]
        public async Task<ActionResult<int>> Create([FromBody] ReceiptDto dto)
        {
            var id = await _receiptService.CreateAsync(dto);
            return Ok(id);
        }

        /// <summary>
        /// Обновить данные приходного документа.
        /// </summary>
        /// <param name="dto">Данные приходного документа для обновления.</param>
        /// <returns>Статус успешности операции (true/false).</returns>
        [HttpPut("Update")]
        public async Task<ActionResult<bool>> Update([FromBody] ReceiptDto dto)
        {
            var updated = await _receiptService.UpdateAsync(dto);
            return Ok(updated);
        }

        /// <summary>
        /// Удалить приходной документ.
        /// </summary>
        /// <param name="id">Идентификатор приходного документа.</param>
        /// <returns>
        /// true — если удаление прошло успешно;
        /// 404 — если документ не найден.
        /// </returns>
        [HttpDelete("Delete")]
        public async Task<ActionResult<bool>> Delete(int id)
        {
            var result = await _receiptService.DeleteAsnc(id);
            return result switch
            {
                DeleteResourceResult.Success => Ok(true),
                DeleteResourceResult.NotFound => NotFound($"Приходной документ с идентификатором {id} не найден.")

            };
        }

        /// <summary>
        /// Получить все фильтры для приходных документов.
        /// </summary>
        /// <returns>Объект FilterReceiptRequiresDto с необходимыми данными для фильтрации.</returns>
        [HttpGet("GetAllFitters")]
        public async Task<ActionResult<FilterReceiptRequiresDto>> GetAllFitters()
        {
            var result = await _receiptService.GetAllFitters();
            return Ok(result);
        }

    }
}