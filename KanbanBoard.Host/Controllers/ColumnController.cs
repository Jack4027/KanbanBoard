using KanbanBoard.Application.DTOs.Requests.Column;
using KanbanBoard.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace KanbanBoard.Host.Controllers
{
    /// Controller responsible for handling all the column related operations, it uses the IColumnService to perform the business logic and returns the appropriate responses to the client.
    [ApiController]
    [Route("api")]
    [Authorize]
    public class ColumnController(IColumnService columnService) : ControllerBase
    {
        // Property to get the user id from the claims, this is used to ensure that users can only access their own columns and not the columns of other users
        private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        // Action method to create a new column, it takes the board id and the create column dto as parameters, it returns the created column as a response
        [HttpPost("boards/{boardId}/columns")]
        public async Task<IActionResult> CreateColumn(Guid boardId, CreateColumnDto dto)
        {
            var response = await columnService.CreateColumn(boardId, dto, UserId);
            return Ok(response);
        }

        // Action method to update an existing column, it takes the column id and the update column dto as parameters, it returns the updated column as a response
        [HttpPut("columns/{id}")]
        public async Task<IActionResult> UpdateColumn(Guid id, UpdateColumnDto dto)
        {
            var response = await columnService.UpdateColumn(id, dto, UserId);
            return Ok(response);
        }

        // Action method to delete a column, it takes the column id as a parameter, it returns a 204 No Content response if the deletion is successful
        [HttpDelete("columns/{id}")]
        public async Task<IActionResult> DeleteColumn(Guid id)
        {
            await columnService.DeleteColumn(id, UserId);
            return NoContent();
        }
    }
}
