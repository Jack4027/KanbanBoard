using KanbanBoard.Application.DTOs.Requests;
using KanbanBoard.Application.DTOs.Requests.Board;
using KanbanBoard.Application.DTOs.Requests.Pagination;
using KanbanBoard.Application.DTOs.Responses;
using KanbanBoard.Application.DTOs.Responses.Pagination;
using System;
using System.Collections.Generic;
using System.Text;

namespace KanbanBoard.Application.Interfaces.Services
{
    //Interface used to inject the BoardService in the controllers,
    //it defines the methods that will be implemented in the service to handle the business logic of the board operations such as creating, updating, deleting and getting boards.
    public interface IBoardService
    {
        Task<BoardResponseDto> CreateBoard(CreateBoardDto dto, string userId);
        Task<BoardResponseDto> GetBoardById(Guid id, string userId);
        Task<PagedResult<BoardSummaryResponseDto>> GetBoardsByUserId(string userId, PaginationParams pagination);
        Task<BoardResponseDto> UpdateBoard(Guid id, UpdateBoardDto dto, string userId);
        Task DeleteBoard(Guid id, string userId);
    }
}
