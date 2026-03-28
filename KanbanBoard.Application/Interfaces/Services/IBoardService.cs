using KanbanBoard.Application.DTOs.Requests;
using KanbanBoard.Application.DTOs.Responses;
using System;
using System.Collections.Generic;
using System.Text;

namespace KanbanBoard.Application.Interfaces.Services
{
    public interface IBoardService
    {
        Task<BoardResponseDto> CreateBoard(CreateBoardDto dto, string userId);
        Task<BoardResponseDto> GetBoardById(Guid id, string userId);
        Task<IEnumerable<BoardSummaryResponseDto>> GetBoardsByUserId(string userId);
        Task<BoardResponseDto> UpdateBoard(Guid id, UpdateBoardDto dto, string userId);
        Task DeleteBoard(Guid id, string userId);
    }
}
