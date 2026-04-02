using KanbanBoard.Application.DTOs.Requests;
using KanbanBoard.Application.DTOs.Requests.Board;
using KanbanBoard.Application.DTOs.Requests.Pagination;
using KanbanBoard.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace KanbanBoard.Host.Controllers
{
    /// Controller responsible for handling all the board related operations, it uses the IBoardService to perform the business logic and returns the appropriate responses to the client.
    // The controller is decorated with the [Authorize] attribute to ensure that only authenticated users can access its endpoints
    [ApiController]
    [Route("api/boards")]
    [Authorize]
    public class BoardController(IBoardService boardService) : ControllerBase
    {
        // Property to get the user id from the claims, this is used to ensure that users can only access their own boards and not the boards of other users
        private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        // Endpoint to get the list of boards for the authenticated user, it accepts pagination parameters in the query string and returns a paginated list of board summaries
        [HttpGet]
        public async Task<IActionResult> GetBoards([FromQuery] PaginationParams pagination)
        {
            var response = await boardService.GetBoardsByUserId(UserId, pagination);
            return Ok(response);
        }

        // Endpoint to get a specific board by its id, it checks if the board belongs to the authenticated user and returns the board details if it does
        [HttpGet("{id}")]
        public async Task<IActionResult> GetBoardById(Guid id)
        {
            var response = await boardService.GetBoardById(id, UserId);
            return Ok(response);
        }

        // Endpoint to create a new board, it accepts the board details in the request body and returns the created board with its id and other details
        [HttpPost]
        public async Task<IActionResult> CreateBoard(CreateBoardDto dto)
        {
            var response = await boardService.CreateBoard(dto, UserId);
            // The CreatedAtAction method is used to return a 201 Created response with the location of the newly created board, it also includes the board details in the response body
            return CreatedAtAction(nameof(GetBoardById), new { id = response.Id }, response);
        }

        // Endpoint to update an existing board, it checks if the board belongs to the authenticated user and updates the board details if it does, it returns the updated board details in the response
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBoard(Guid id, UpdateBoardDto dto)
        {
            var response = await boardService.UpdateBoard(id, dto, UserId);

            // The Ok method is used to return a 200 OK response with the updated board details in the response body
            // If unsuccessful (eg. if the board does not belong to the user or if the board does not exist), the service will throw an exception that will be handled by the global exception handler, which will return an appropriate error response to the client
            return Ok(response);
        }


        // Endpoint to delete a board, it checks if the board belongs to the authenticated user and deletes the board if it does, it returns a 204 No Content response if the deletion is successful
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBoard(Guid id)
        {
            await boardService.DeleteBoard(id, UserId);
            return NoContent();
        }
    }
}
