using Warehouse.Common;
using Warehouse.Services.DTO;
using Warehouse.Services.Iservices;
using Microsoft.AspNetCore.Mvc;


namespace Warehouse.Controllers
{
    //[Authorize(Roles = "Admin")]
    [ApiController]
    [Route("[controller]")]
    public class UnitController : ControllerBase
    {
        private readonly IUnitService _unitService;

        public UnitController(IUnitService repositoryServices)
        {
            _unitService = repositoryServices;
        }

        /// <summary>
        /// Получить список всех единиц измерения.
        /// </summary>
        /// <returns>Список объектов UnitDto.</returns>
        [HttpGet("GetAll")]
        public async Task<ActionResult<List<UnitDto>>> GetAll()
        {
            var result = await _unitService.GetAllAsync();
            return Ok(result);
        }

        /// <summary>
        /// Получить единицу измерения по идентификатору.
        /// </summary>
        /// <param name="id">Идентификатор единицы измерения.</param>
        /// <returns>Объект UnitDto, если единица найдена; иначе 404.</returns>
        [HttpGet("GetById")]
        public async Task<ActionResult<UnitDto>> GetById(int id)
        {
            var unit = await _unitService.GetByIdAsync(id);
            if (unit == null) return NotFound();
            return Ok(unit);
        }

        /// <summary>
        /// Получить список единиц измерения по статусу.
        /// </summary>
        /// <param name="status">Статус единицы (enum STATUS).</param>
        /// <returns>Список единиц измерения с указанным статусом.</returns>
        [HttpGet("GetByStatus")]
        public async Task<ActionResult<List<UnitDto>>> GetByStatus(STATUS status)
        {
            var result = await _unitService.GetByStatusAsync(status);
            return Ok(result);
        }

        /// <summary>
        /// Создать новую единицу измерения.
        /// </summary>
        /// <param name="dto">Данные единицы для создания.</param>
        /// <returns>Идентификатор созданной единицы измерения.</returns>
        [HttpPost("Create")]
        public async Task<ActionResult<int>> Create(UnitDto dto)
        {
            var id = await _unitService.CreateAsync(dto);
            return id;
        }

        /// <summary>
        /// Обновить данные единицы измерения.
        /// </summary>
        /// <param name="dto">Данные единицы для обновления.</param>
        /// <returns>Статус успешности операции (true/false).</returns>
        [HttpPut("Update")]
        public async Task<ActionResult<bool>> Update([FromBody] UnitDto dto)
        {
            var updated = await _unitService.UpdateAsync(dto);
            return updated;
        }

        /// <summary>
        /// Изменить статус единицы измерения.
        /// </summary>
        /// <param name="id">Идентификатор единицы измерения.</param>
        /// <returns>
        /// true — если статус успешно изменён;
        /// 404 — если единица не найдена.
        /// </returns>
        [HttpPut("ChangeStatus")]
        public async Task<ActionResult<bool>> ChangeStatus(int id)
        {
            var result = await _unitService.ChangeStatusAsync(id);
            return result switch
            {
                DeleteResourceResult.Success => Ok(true),
                DeleteResourceResult.NotFound => NotFound($"Единица с идентификатором {id} не найдена.")
            };
        }

        /// <summary>
        /// Удалить единицу измерения.
        /// </summary>
        /// <param name="id">Идентификатор единицы измерения.</param>
        /// <returns>
        /// true — если удаление прошло успешно;
        /// 404 — если единица не найдена;
        /// 400 — если удаление невозможно из-за зависимостей;
        /// 500 — внутренняя ошибка сервера.
        /// </returns>
        [HttpDelete("Delete")]
        public async Task<ActionResult<bool>> Delete(int id)
        {
            var result = await _unitService.DeleteAsnc(id);
            return result switch
            {
                DeleteResourceResult.Success => Ok(true),
                DeleteResourceResult.NotFound => NotFound($"Единица с идентификатором {id} не найдена."),
                DeleteResourceResult.HasDependencies => BadRequest("Невозможно удалить единицу, так как имеются зависимости."),
                _ => StatusCode(500)
            };
        }


    }


}
