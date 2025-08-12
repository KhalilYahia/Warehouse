using Warehouse.Common;
using Warehouse.Services.DTO;
using Warehouse.Services.Iservices;
using Microsoft.AspNetCore.Mvc;


namespace Warehouse.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public class BalanceController : ControllerBase
    {
        private readonly IBalanceService _balanceService;

        public BalanceController(IBalanceService balanceService)
        {
            _balanceService = balanceService;
        }

        /// <summary>
        /// Выполняет поиск балансов по заданным критериям.
        /// </summary>
        /// <param name="dto">Объект с параметрами поиска (SearchInBalanceDto).</param>
        /// <returns>Список найденных объектов BalanceDto.</returns>
        [HttpPost("Search")]
        public async Task<ActionResult<List<BalanceDto>>> Search([FromBody] SearchInBalanceDto dto)
        {
            var result = await _balanceService.Search(dto);
            return Ok(result);
        }

        /// <summary>
        /// Получает все активные элементы.
        /// </summary>
        /// <returns>Объект AllActiveElementsDto, содержащий все активные элементы.</returns>
        [HttpGet("GetAllActiveElements")]
        public async Task<ActionResult<AllActiveElementsDto>> GetAllActiveElements()
        {
            var result = await _balanceService.GetAllActiveElements();
            return Ok(result);
        }

        /// <summary>
        /// Получает все активированные балансы.
        /// </summary>
        /// <returns>Объект BalanceDto с информацией по всем активированным балансам.</returns>
        [HttpGet("GetAllActivatedInBalance")]
        public async Task<ActionResult<BalanceDto>> GetAllActivatedInBalance()
        {
            var result = await _balanceService.GetAllActivatedInBalance();
            return Ok(result);
        }

    }


}
