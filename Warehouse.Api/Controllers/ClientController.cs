using Warehouse.Common;
using Warehouse.Services.DTO;
using Warehouse.Services.Iservices;
using Microsoft.AspNetCore.Mvc;


namespace Warehouse.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ClientController : ControllerBase
    {
        private readonly IClientService _clientService;

        public ClientController(IClientService clientService)
        {
            _clientService = clientService;
        }

        /// <summary>
        /// Получить список всех клиентов.
        /// </summary>
        /// <returns>Список объектов ClientDto.</returns>
        [HttpGet("GetAll")]
        public async Task<ActionResult<List<ClientDto>>> GetAll()
        {
            var result = await _clientService.GetAllAsync();
            return Ok(result);
        }

        /// <summary>
        /// Получить клиента по идентификатору.
        /// </summary>
        /// <param name="id">Идентификатор клиента.</param>
        /// <returns>Объект ClientDto, если клиент найден; иначе статус 404.</returns>
        [HttpGet("GetById")]
        public async Task<ActionResult<ClientDto>> GetById(int id)
        {
            var client = await _clientService.GetByIdAsync(id);
            if (client == null) return NotFound();
            return Ok(client);
        }

        /// <summary>
        /// Получить список клиентов по статусу.
        /// </summary>
        /// <param name="status">Статус клиента (enum STATUS).</param>
        /// <returns>Список клиентов с указанным статусом.</returns>
        [HttpGet("GetByStatus")]
        public async Task<ActionResult<List<ClientDto>>> GetByStatus(STATUS status)
        {
            var result = await _clientService.GetByStatusAsync(status);
            return Ok(result);
        }

        /// <summary>
        /// Создать нового клиента.
        /// </summary>
        /// <param name="dto">Данные клиента для создания.</param>
        /// <returns>Идентификатор созданного клиента.</returns>
        [HttpPost("Create")]
        public async Task<ActionResult<int>> Create([FromBody] ClientDto dto)
        {
            var id = await _clientService.CreateAsync(dto);
            return Ok(id);
        }

        /// <summary>
        /// Обновить данные клиента.
        /// </summary>
        /// <param name="dto">Данные клиента для обновления.</param>
        /// <returns>Статус успешности операции.</returns>
        [HttpPut("Update")]
        public async Task<ActionResult<bool>> Update([FromBody] ClientDto dto)
        {
            var updated = await _clientService.UpdateAsync(dto);
            return Ok(updated);
        }

        /// <summary>
        /// Изменить статус клиента.
        /// </summary>
        /// <param name="id">Идентификатор клиента.</param>
        /// <returns>
        /// true — если статус изменён успешно;
        /// 404 — если клиент не найден;
        /// 500 — ошибка сервера.
        /// </returns>
        [HttpPut("ChangeStatus")]
        public async Task<ActionResult<bool>> ChangeStatus(int id)
        {
            var result = await _clientService.ChangeStatusAsync(id);
            return result switch
            {
                DeleteResourceResult.Success => Ok(true),
                DeleteResourceResult.NotFound => NotFound($"Клиент с идентификатором {id} не найден."),
                _ => StatusCode(500)
            };
        }

        /// <summary>
        /// Удалить клиента.
        /// </summary>
        /// <param name="id">Идентификатор клиента.</param>
        /// <returns>
        /// true — если удаление прошло успешно;
        /// 404 — если клиент не найден;
        /// 400 — если удаление невозможно из-за зависимостей;
        /// 500 — ошибка сервера.

        [HttpDelete("Delete")]
        public async Task<ActionResult<bool>> Delete(int id)
        {
            var result = await _clientService.DeleteAsnc(id);
            return result switch
            {
                DeleteResourceResult.Success => Ok(true),
                DeleteResourceResult.NotFound => NotFound($"Клиент с идентификатором {id} не найден."),
                DeleteResourceResult.HasDependencies => BadRequest("Невозможно удалить клиента, так как имеются зависимости."),
                _ => StatusCode(500)
            };
        }
    }


}
