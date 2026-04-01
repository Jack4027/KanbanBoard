using KanbanBoard.Application.DTOs.Requests.Card;
using KanbanBoard.Application.Interfaces.Services;
using KanbanBoard.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.Metrics;
using System.Security.Claims;

namespace KanbanBoard.Host.Controllers
{
    [ApiController]
    [Route("api")]
    [Authorize]
    public class CardController(ICardService cardService) : ControllerBase
    {
        private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        [HttpPost("columns/{columnId}/cards")]
        public async Task<IActionResult> CreateCard(Guid columnId, CreateCardDto dto)
        {
            var response = await cardService.CreateCard(columnId, dto, UserId);
            return Ok(response);
        }

        [HttpPut("cards/{id}")]
        public async Task<IActionResult> UpdateCard(Guid id, UpdateCardDto dto)
        {
            var response = await cardService.UpdateCard(id, dto, UserId);
            return Ok(response);
        }

        [HttpPost("cards/{id}/move")]
        public async Task<IActionResult> MoveCard(Guid id, MoveCardDto dto)
        {
            var response = await cardService.MoveCard(id, dto, UserId);
            return Ok(response);
        }

        [HttpDelete("cards/{id}")]
        public async Task<IActionResult> DeleteCard(Guid id)
        {
            await cardService.DeleteCard(id, UserId);
            return NoContent();
        }
    }
}
