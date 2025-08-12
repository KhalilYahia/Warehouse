using Warehouse.Common;
using Warehouse.Services.DTO;
using Warehouse.Services.Iservices;
using Microsoft.AspNetCore.Mvc;

namespace Warehouse.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public class ShipmentController : ControllerBase
    {
        private readonly IShipmentsService _shipmentService;

        public ShipmentController(IShipmentsService shipmentService)
        {
            _shipmentService = shipmentService;
        }

        /// <summary>
        /// Поиск документов отгрузки по заданным параметрам.
        /// </summary>
        /// <param name="dto">Параметры поиска (SearchInShipmentsDto).</param>
        /// <returns>Список найденных документов отгрузки (ShipmentDto).</returns>
        [HttpPost("Search")]
        public async Task<ActionResult<List<ShipmentDto>>> Search([FromBody] SearchInShipmentsDto dto)
        {
            var result = await _shipmentService.Search(dto);
            return Ok(result);
        }

        /// <summary>
        /// Получить документ отгрузки по идентификатору.
        /// </summary>
        /// <param name="OutboundDocument_Id">Идентификатор документа отгрузки.</param>
        /// <returns>Объект ShipmentDto, если документ найден; иначе 404 с сообщением.</returns>
        [HttpGet("GetById")]
        public async Task<ActionResult<ShipmentDto>> GetById(int OutboundDocument_Id)
        {
            var shipment = await _shipmentService.GetByIdAsync(OutboundDocument_Id);
            if (shipment == null)
                return NotFound($"Документ отгрузки с идентификатором {OutboundDocument_Id} не найден.");
            return Ok(shipment);
        }

        //// <summary>
        /// Создать новый документ отгрузки.
        /// </summary>
        /// <param name="dto">Данные документа для создания.</param>
        /// <returns>Идентификатор созданного документа.</returns>
        [HttpPost("Create")]
        public async Task<ActionResult<int>> Create([FromBody] ShipmentDto dto)
        {
            var id = await _shipmentService.CreateAsync(dto);
            return Ok(id);
        }

        /// <summary>
        /// Обновить данные документа отгрузки.
        /// </summary>
        /// <param name="dto">Данные документа для обновления.</param>
        /// <returns>Статус успешности операции (true/false).</returns>
        [HttpPut("Update")]
        public async Task<ActionResult<bool>> Update([FromBody] ShipmentDto dto)
        {
            var updated = await _shipmentService.UpdateAsync(dto);
            return Ok(updated);
        }

        /// <summary>
        /// Снять подпись с документа отгрузки.
        /// </summary>
        /// <param name="OutboundDocumentId">Идентификатор документа отгрузки.</param>
        /// <returns>
        /// true — если подпись успешно снята;
        /// 404 — если документ не найден;
        /// 400 — если документ уже подписан и нельзя снять подпись;
        /// 500 — внутренняя ошибка сервера.
        /// </returns>
        [HttpPut("UnSign")]
        public async Task<ActionResult<bool>> UnSign(int OutboundDocumentId)
        {
            var result = await _shipmentService.UnSign(OutboundDocumentId);
            return result switch
            {
                DeleteResourceResult.Success => Ok(true),
                DeleteResourceResult.NotFound => NotFound($"Документ отгрузки с идентификатором {OutboundDocumentId} не найден."),
                DeleteResourceResult.HasDependencies => BadRequest("Этот документ уже подписан"),
                _ => StatusCode(500)
            };
        }

        /// <summary>
        /// Удалить документ отгрузки.
        /// </summary>
        /// <param name="id">Идентификатор документа отгрузки.</param>
        /// <returns>
        /// true — если удаление прошло успешно;
        /// 404 — если документ не найден;
        /// 400 — если удаление невозможно из-за подписанного статуса;
        /// 500 — внутренняя ошибка сервера.
        /// </returns>
        [HttpDelete("Delete")]
        public async Task<ActionResult<bool>> Delete(int id)
        {
            var result = await _shipmentService.DeleteAsnc(id);
            return result switch
            {
                DeleteResourceResult.Success => Ok(true),
                DeleteResourceResult.NotFound => NotFound($"Документ отгрузки с идентификатором {id} не найден."),
                DeleteResourceResult.HasDependencies => BadRequest("Вы не можете удалить подписанный документ"),
                 _ => StatusCode(500)


            };
        }
    }
}