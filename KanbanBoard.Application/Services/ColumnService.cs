using AutoMapper;
using KanbanBoard.Application.DTOs.Responses;
using KanbanBoard.Domain.Interfaces.Repository;
using KanbanBoard.Application.Interfaces.Services;
using KanbanBoard.Domain.Entities;
using KanbanBoard.Domain.Interfaces.Repository;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using KanbanBoard.Application.DTOs.Requests.Column;


namespace KanbanBoard.Application.Services
{
    public class ColumnService(
        IColumnRepository columnRepository,
        IBoardRepository boardRepository,
        IBoardMemberRepository boardMemberRepository,
        IMapper mapper,
        ILogger<ColumnService> logger) : IColumnService
    {
        public async Task<ColumnResponseDto> CreateColumn(Guid boardId, CreateColumnDto dto, string userId)
        {
            var board = await boardRepository.GetById(boardId)
                ?? throw new KeyNotFoundException($"Board with Id {boardId} not found.");

            var membership = await boardMemberRepository.GetByBoardAndUser(boardId, userId);
            if (membership == null || membership.Role != "Admin")
                throw new UnauthorizedAccessException("Only board Admins can create columns.");

            // Check for duplicate column name on this board
            var duplicateName = await columnRepository.ExistsWithNameOnBoard(boardId, dto.Name);

            if (duplicateName)
            {
                throw new InvalidOperationException($"A column with the given name - {dto.Name} - already exists");
            }

            var position = board.Columns.Count;

            var column = new Column
            {
                Id = Guid.NewGuid(),
                BoardId = boardId,
                Name = dto.Name,
                Position = position
            };

            var saved = await columnRepository.Add(column);

            logger.LogInformation("Column {Name} created on board {BoardId}", column.Name, boardId);

            return mapper.Map<ColumnResponseDto>(saved);
        }

        public async Task<ColumnResponseDto> UpdateColumn(Guid id, UpdateColumnDto dto, string userId)
        {
            var column = await columnRepository.GetById(id)
                ?? throw new KeyNotFoundException($"Column with Id {id} not found.");

            var membership = await boardMemberRepository.GetByBoardAndUser(column.BoardId, userId);
            if (membership == null || membership.Role != "Admin")
                throw new UnauthorizedAccessException("Only board Admins can update columns.");

            // Load the board to check for duplicate names
            var board = await boardRepository.GetById(column.BoardId)
                ?? throw new KeyNotFoundException("Board not found.");

            // Check for duplicate column name on this board
            var duplicateName = await columnRepository.ExistsWithNameOnBoard(column.BoardId, dto.Name, id);

            if (duplicateName)
            {
                throw new InvalidOperationException($"A column with the given name - {dto.Name} - already exists");
            }

            column.Name = dto.Name;

            var updated = await columnRepository.Update(column);

            logger.LogInformation("Column {Id} updated by user {UserId}", id, userId);

            return mapper.Map<ColumnResponseDto>(updated);
        }

        public async Task DeleteColumn(Guid id, string userId)
        {
            var column = await columnRepository.GetById(id)
                ?? throw new KeyNotFoundException($"Column with Id {id} not found.");

            var membership = await boardMemberRepository.GetByBoardAndUser(column.BoardId, userId);
            if (membership == null || membership.Role != "Admin")
                throw new UnauthorizedAccessException("Only board Admins can delete columns.");

            await columnRepository.Delete(id);

            logger.LogInformation("Column {Id} deleted by user {UserId}", id, userId);
        }
    }
}
