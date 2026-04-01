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
    public interface IBoardService
    {
        Task<BoardResponseDto> CreateBoard(CreateBoardDto dto, string userId);
        Task<BoardResponseDto> GetBoardById(Guid id, string userId);
        Task<PagedResult<BoardSummaryResponseDto>> GetBoardsByUserId(string userId, PaginationParams pagination);
        Task<BoardResponseDto> UpdateBoard(Guid id, UpdateBoardDto dto, string userId);
        Task DeleteBoard(Guid id, string userId);
    }
}
