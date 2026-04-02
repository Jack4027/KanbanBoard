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
    // Controller responsible for handling all the card related operations, it uses the ICardService to perform the business logic and returns the appropriate responses to the client.
    [ApiController]
    [Route("api")]
    [Authorize]
    public class CardController(ICardService cardService) : ControllerBase
    {
        // Property to get the user id from the claims, this is used to ensure that users can only access their own cards and not the cards of other users
        private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        // Endpoint to create a new card in a specific column, it accepts the card details in the request body and returns the created card with its id and other details
        [HttpPost("columns/{columnId}/cards")]
        public async Task<IActionResult> CreateCard(Guid columnId, CreateCardDto dto)
        {
            var response = await cardService.CreateCard(columnId, dto, UserId);
            return Ok(response);
        }

        // Endpoint to update an existing card, it accepts the card id in the route and the updated card details in the request body, it returns the updated card with its new details
        [HttpPut("cards/{id}")]
        public async Task<IActionResult> UpdateCard(Guid id, UpdateCardDto dto)
        {
            var response = await cardService.UpdateCard(id, dto, UserId);
            return Ok(response);
        }

        // Endpoint to move a card from one column to another, it accepts the card id in the route and the new column id in the request body, it returns the updated card with its new column and position
        [HttpPost("cards/{id}/move")]
        public async Task<IActionResult> MoveCard(Guid id, MoveCardDto dto)
        {
            var response = await cardService.MoveCard(id, dto, UserId);
            return Ok(response);
        }

        // Endpoint to delete a card, it accepts the card id in the route and deletes the card if it belongs to the authenticated user, it returns a 204 No Content response if the deletion is successful
        [HttpDelete("cards/{id}")]
        public async Task<IActionResult> DeleteCard(Guid id)
        {
            await cardService.DeleteCard(id, UserId);
            return NoContent();
        }
    }
}
