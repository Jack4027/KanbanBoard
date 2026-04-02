using AutoMapper;
using KanbanBoard.Application.DTOs.Requests.Board;
using KanbanBoard.Application.DTOs.Requests.Pagination;
using KanbanBoard.Application.DTOs.Responses;
using KanbanBoard.Application.DTOs.Responses.Pagination;
using KanbanBoard.Application.Interfaces.Services;
using KanbanBoard.Domain.Entities;
using KanbanBoard.Domain.Interfaces.Repository;
using Microsoft.Extensions.Logging;

namespace KanbanBoard.Application.Services
{
    //This service implements the IBoardService interface and contains the business logic for managing boards, such as creating, updating, deleting and retrieving boards.
    //It uses the IBoardRepository to interact with the database and the IBoardMemberRepository to check board memberships and roles. It also uses AutoMapper to map between entities and DTOs,
    //and ILogger to log important actions such as board creation, updates and deletions.
    public class BoardService(
        IBoardRepository boardRepository,
        IBoardMemberRepository boardMemberRepository,
        IMapper mapper,
        ILogger<BoardService> logger) : IBoardService
    {
        //Service logic to create a board via a call to the repository, ensuring the CreatedBy is set from the userId in the token
        public async Task<BoardResponseDto> CreateBoard(CreateBoardDto dto, string userId)
        {
            //Create the EFCore entity and set the CreatedBy to the userId from the token
            var board = new Board
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                CreatedBy = userId
            };

            // Add membership to the board before saving
            // EF Core saves both in one SaveChangesAsync call
            board.Members.Add(new BoardMember
            {
                BoardId = board.Id,
                UserId = userId,
                Role = "Admin"
            });

            //Call the repository to save the board and return the saved entity
            var savedBoard = await boardRepository.Add(board);

            //Log this action with the board name and userId
            logger.LogInformation("Board {Name} created by user {UserId}", board.Name, userId);

            //Return the mapped response from the saved entity
            return mapper.Map<BoardResponseDto>(savedBoard);
        }

        //Service logic to get a board by id, ensuring the user is a member of the board before returning it
        public async Task<BoardResponseDto> GetBoardById(Guid id, string userId)
        {
            //Call the repository to get the board by id, if not found throw a KeyNotFoundException
            var board = await boardRepository.GetById(id)
                ?? throw new KeyNotFoundException($"Board with Id {id} not found.");

            //Check if the user is a member of the board by calling the BoardMemberRepository, if not throw an UnauthorizedAccessException
            var membership = await boardMemberRepository.GetByBoardAndUser(id, userId);
            if (membership == null)
                throw new UnauthorizedAccessException("You do not have access to this board.");

            //Return the mapped response from the entity
            return mapper.Map<BoardResponseDto>(board);
        }

        //Service logic to get all boards for a user, ensuring only boards where the user is a member are returned, and supporting pagination
        public async Task<PagedResult<BoardSummaryResponseDto>> GetBoardsByUserId(string userId, PaginationParams pagination)
        {
            //Call the repository to get the boards for the user with pagination,
            //this will return only the boards where the user is a member,
            //and also return the total count of boards for that user for pagination metadata
            var (items, totalCount) = await boardRepository.GetByUserId(
                userId,
                pagination.Page,
                pagination.PageSize);

            //Return the mapped response with the items and pagination metadata
            return new PagedResult<BoardSummaryResponseDto>
            {
                Items = mapper.Map<IEnumerable<BoardSummaryResponseDto>>(items),
                TotalCount = totalCount,
                Page = pagination.Page,
                PageSize = pagination.PageSize
            };
        }

        //Service logic to update a board, ensuring only board Admins can update the board, and if the board is not found throw a KeyNotFoundException
        public async Task<BoardResponseDto> UpdateBoard(Guid id, UpdateBoardDto dto, string userId)
        {
            // Call the repository to get the board by id, if not found throw a KeyNotFoundException
            var board = await boardRepository.GetById(id)
                ?? throw new KeyNotFoundException($"Board with Id {id} not found.");

            //Using the composite key of BoardId and UserId, check if the user is a member of the board and has the Admin role by calling the BoardMemberRepository,
            var membership = await boardMemberRepository.GetByBoardAndUser(id, userId);
            if (membership == null || membership.Role != "Admin")
                throw new UnauthorizedAccessException("Only board Admins can update this board.");

            //Update the board properties from the DTO
            board.Name = dto.Name;

            //Call the repository to update the board and return the updated entity
            var updated = await boardRepository.Update(board);

            //Log this action with the board id and userId
            logger.LogInformation("Board {Id} updated by user {UserId}", id, userId);

            //Return the mapped response from the updated entity
            return mapper.Map<BoardResponseDto>(updated);
        }

        //Service logic to delete a board, ensuring only board Admins can delete the board, and if the board is not found throw a KeyNotFoundException
        public async Task DeleteBoard(Guid id, string userId)
        {
            // Call the repository to get the board by id, if not found throw a KeyNotFoundException
            var board = await boardRepository.GetById(id)
                ?? throw new KeyNotFoundException($"Board with Id {id} not found.");

            //Using the composite key of BoardId and UserId, check if the user is a member of the board and has the Admin role by calling the BoardMemberRepository,
            var membership = await boardMemberRepository.GetByBoardAndUser(id, userId);
            if (membership == null || membership.Role != "Admin")
                throw new UnauthorizedAccessException("Only board Admins can delete this board.");

            //Call the repository to delete the board
            await boardRepository.Delete(id);

            //  Log this action with the board id and userId
            logger.LogInformation("Board {Id} deleted by user {UserId}", id, userId);

            //No return value needed for delete
        }
    }

}
