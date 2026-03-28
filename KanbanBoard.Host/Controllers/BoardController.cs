using KanbanBoard.Application.DTOs.Requests;
using KanbanBoard.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace KanbanBoard.Host.Controllers
{
    [ApiController]
    [Route("api/boards")]
    [Authorize]
    public class BoardController(IBoardService boardService) : ControllerBase
    {
        private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        [HttpGet]
        public async Task<IActionResult> GetBoards()
        {
            var response = await boardService.GetBoardsByUserId(UserId);
            return Ok(response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetBoardById(Guid id)
        {
            var response = await boardService.GetBoardById(id, UserId);
            return Ok(response);
        }

        [HttpPost]
        public async Task<IActionResult> CreateBoard(CreateBoardDto dto)
        {
            var response = await boardService.CreateBoard(dto, UserId);
            return CreatedAtAction(nameof(GetBoardById), new { id = response.Id }, response);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBoard(Guid id, UpdateBoardDto dto)
        {
            var response = await boardService.UpdateBoard(id, dto, UserId);
            return Ok(response);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBoard(Guid id)
        {
            await boardService.DeleteBoard(id, UserId);
            return NoContent();
        }
    }
}
