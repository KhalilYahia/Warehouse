using Warehouse.Common;
using Warehouse.Services.DTO;
using Warehouse.Services.Iservices;
using Microsoft.AspNetCore.Mvc;


namespace Warehouse.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ResourceController : ControllerBase
    {
        private readonly IResourceService _resourceService;

        public ResourceController(IResourceService resourceService)
        {
            _resourceService = resourceService;
        }

        /// <summary>
        /// Получить список всех ресурсов.
        /// </summary>
        /// <returns>Список объектов ResourceDto.</returns>
        [HttpGet("GetAll")]
        public async Task<ActionResult<List<ResourceDto>>> GetAll()
        {
            var result = await _resourceService.GetAllAsync();
            return Ok(result);
        }

        /// <summary>
        /// Получить ресурс по идентификатору.
        /// </summary>
        /// <param name="id">Идентификатор ресурса.</param>
        /// <returns>Объект ResourceDto, если ресурс найден; иначе 404.</returns>
        [HttpGet("GetById")]
        public async Task<ActionResult<ResourceDto>> GetById(int id)
        {
            var resource = await _resourceService.GetByIdAsync(id);
            if (resource == null) return NotFound();
            return Ok(resource);
        }

        /// <summary>
        /// Получить список ресурсов по статусу.
        /// </summary>
        /// <param name="status">Статус ресурса (enum STATUS).</param>
        /// <returns>Список ресурсов с указанным статусом.</returns>
        [HttpGet("GetByStatus")]
        public async Task<ActionResult<List<ResourceDto>>> GetByStatus(STATUS status)
        {
            var result = await _resourceService.GetByStatusAsync(status);
            return Ok(result);
        }

        /// <summary>
        /// Создать новый ресурс.
        /// </summary>
        /// <param name="dto">Данные ресурса для создания.</param>
        /// <returns>Идентификатор созданного ресурса.</returns>
        [HttpPost("Create")]
        public async Task<ActionResult<int>> Create([FromBody] ResourceDto dto)
        {
            var id = await _resourceService.CreateAsync(dto);
            return Ok(id);
        }

        /// <summary>
        /// Обновить данные ресурса.
        /// </summary>
        /// <param name="dto">Данные ресурса для обновления.</param>
        /// <returns>Статус успешности операции (true/false).</returns>
        [HttpPut("Update")]
        public async Task<ActionResult<bool>> Update([FromBody] ResourceDto dto)
        {
            var updated = await _resourceService.UpdateAsync(dto);
            return Ok(updated);
        }

        /// <summary>
        /// Изменить статус ресурса.
        /// </summary>
        /// <param name="id">Идентификатор ресурса.</param>
        /// <returns>
        /// true — если статус изменён успешно;
        /// 404 — если ресурс не найден.
        /// </returns>
        [HttpPut("ChangeStatus")]
        public async Task<ActionResult<bool>> ChangeStatus(int id)
        {
            var result = await _resourceService.ChangeStatusAsync(id);
            return result switch
            {
                DeleteResourceResult.Success => Ok(true),
                DeleteResourceResult.NotFound => NotFound($"Ресурс с идентификатором {id} не найден.")                
            };
        }

        /// <summary>
        /// Удалить ресурс.
        /// </summary>
        /// <param name="id">Идентификатор ресурса.</param>
        /// <returns>
        /// true — если удаление прошло успешно;
        /// 404 — если ресурс не найден;
        /// 400 — если удаление невозможно из-за зависимостей;
        /// 500 — внутренняя ошибка сервера.
        /// </returns>
        [HttpDelete("Delete")]
        public async Task<ActionResult<bool>> Delete(int id)
        {
            var result = await _resourceService.DeleteAsnc(id);
            return result switch
            {
                DeleteResourceResult.Success => Ok(true),
                DeleteResourceResult.NotFound => NotFound($"Ресурс с идентификатором {id} не найден."),
                DeleteResourceResult.HasDependencies => BadRequest("Невозможно удалить ресурс, так как имеются зависимости."),
                _ => StatusCode(500)
            };
        }

    }


}
