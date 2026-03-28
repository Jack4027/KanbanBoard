using AutoMapper;
using KanbanBoard.Application.DTOs.Requests;
using KanbanBoard.Application.DTOs.Responses;
using KanbanBoard.Application.Interfaces.Services;
using KanbanBoard.Domain.Entities;
using KanbanBoard.Domain.Interfaces.Repository;
using Microsoft.Extensions.Logging;

namespace KanbanBoard.Application.Services
{
    public class BoardService(
        IBoardRepository boardRepository,
        IBoardMemberRepository boardMemberRepository,
        IMapper mapper,
        ILogger<BoardService> logger) : IBoardService
    {
        public async Task<BoardResponseDto> CreateBoard(CreateBoardDto dto, string userId)
        {
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

            var savedBoard = await boardRepository.Add(board);

            logger.LogInformation("Board {Name} created by user {UserId}", board.Name, userId);

            return mapper.Map<BoardResponseDto>(savedBoard);
        }

        public async Task<BoardResponseDto> GetBoardById(Guid id, string userId)
        {
            var board = await boardRepository.GetById(id)
                ?? throw new KeyNotFoundException($"Board with Id {id} not found.");

            var membership = await boardMemberRepository.GetByBoardAndUser(id, userId);
            if (membership == null)
                throw new UnauthorizedAccessException("You do not have access to this board.");

            return mapper.Map<BoardResponseDto>(board);
        }

        public async Task<IEnumerable<BoardSummaryResponseDto>> GetBoardsByUserId(string userId)
        {
            var boards = await boardRepository.GetByUserId(userId);
            return mapper.Map<IEnumerable<BoardSummaryResponseDto>>(boards);
        }

        public async Task<BoardResponseDto> UpdateBoard(Guid id, UpdateBoardDto dto, string userId)
        {
            var board = await boardRepository.GetById(id)
                ?? throw new KeyNotFoundException($"Board with Id {id} not found.");

            var membership = await boardMemberRepository.GetByBoardAndUser(id, userId);
            if (membership == null || membership.Role != "Admin")
                throw new UnauthorizedAccessException("Only board Admins can update this board.");

            board.Name = dto.Name;

            var updated = await boardRepository.Update(board);

            logger.LogInformation("Board {Id} updated by user {UserId}", id, userId);

            return mapper.Map<BoardResponseDto>(updated);
        }

        public async Task DeleteBoard(Guid id, string userId)
        {
            var board = await boardRepository.GetById(id)
                ?? throw new KeyNotFoundException($"Board with Id {id} not found.");

            var membership = await boardMemberRepository.GetByBoardAndUser(id, userId);
            if (membership == null || membership.Role != "Admin")
                throw new UnauthorizedAccessException("Only board Admins can delete this board.");

            await boardRepository.Delete(id);

            logger.LogInformation("Board {Id} deleted by user {UserId}", id, userId);
        }
    }

}
