using KanbanBoard.Application.DTOs.Requests.Column;
using KanbanBoard.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace KanbanBoard.Host.Controllers
{
    [ApiController]
    [Route("api")]
    [Authorize]
    public class ColumnController(IColumnService columnService) : ControllerBase
    {
        private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        [HttpPost("boards/{boardId}/columns")]
        public async Task<IActionResult> CreateColumn(Guid boardId, CreateColumnDto dto)
        {
            var response = await columnService.CreateColumn(boardId, dto, UserId);
            return Ok(response);
        }

        [HttpPut("columns/{id}")]
        public async Task<IActionResult> UpdateColumn(Guid id, UpdateColumnDto dto)
        {
            var response = await columnService.UpdateColumn(id, dto, UserId);
            return Ok(response);
        }

        [HttpDelete("columns/{id}")]
        public async Task<IActionResult> DeleteColumn(Guid id)
        {
            await columnService.DeleteColumn(id, UserId);
            return NoContent();
        }
    }
}
