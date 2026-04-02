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
    //Service that implements the IColumnService interface, it contains the business logic for creating, updating and deleting columns.
    public class ColumnService(
        IColumnRepository columnRepository,
        IBoardRepository boardRepository,
        IBoardMemberRepository boardMemberRepository,
        IMapper mapper,
        ILogger<ColumnService> logger) : IColumnService
    {
        // The CreateColumn method creates a new column on the specified board, it checks if the user is an admin of the board and if the column name is unique on the board before creating it.
        public async Task<ColumnResponseDto> CreateColumn(Guid boardId, CreateColumnDto dto, string userId)
        {
            // Load the board to check permissions and existing columns
            var board = await boardRepository.GetById(boardId)
                ?? throw new KeyNotFoundException($"Board with Id {boardId} not found.");

            // Check if the user is an admin of the board
            var membership = await boardMemberRepository.GetByBoardAndUser(boardId, userId);

            // Only Admins can create columns
            if (membership == null || membership.Role != "Admin")
            {
                throw new UnauthorizedAccessException("Only board Admins can create columns.");
            }

            // Check for duplicate column name on this board
            var duplicateName = await columnRepository.ExistsWithNameOnBoard(boardId, dto.Name);

            // If a column with the same name already exists, throw an invalid operation exception
            if (duplicateName)
            {
                throw new InvalidOperationException($"A column with the given name - {dto.Name} - already exists");
            }

            // Set the position to the end of the columns list, so that new columns always appear to the right of existing columns
            var position = board.Columns.Count;

            // Create the new column entity
            var column = new Column
            {
                Id = Guid.NewGuid(),
                BoardId = boardId,
                Name = dto.Name,
                Position = position
            };

            // Save the new column to the repository
            var saved = await columnRepository.Add(column);

            // Log the creation of the new column
            logger.LogInformation("Column {Name} created on board {BoardId}", column.Name, boardId);

            // Map the saved column entity to a response DTO and return it
            return mapper.Map<ColumnResponseDto>(saved);
        }

        // The UpdateColumn method updates the name of an existing column, it checks if the user is an admin of the board and if the new column name is unique on the board before updating it.
        public async Task<ColumnResponseDto> UpdateColumn(Guid id, UpdateColumnDto dto, string userId)
        {
            // Load the column to check permissions and existing data
            var column = await columnRepository.GetById(id)
                ?? throw new KeyNotFoundException($"Column with Id {id} not found.");

            // Check if the user is an admin of the board
            var membership = await boardMemberRepository.GetByBoardAndUser(column.BoardId, userId);

            // Only Admins can update columns
            if (membership == null || membership.Role != "Admin")
            {
                throw new UnauthorizedAccessException("Only board Admins can update columns.");
            }

            // Load the board to check for duplicate names
            var board = await boardRepository.GetById(column.BoardId)
                ?? throw new KeyNotFoundException("Board not found.");

            // Check for duplicate column name on this board
            var duplicateName = await columnRepository.ExistsWithNameOnBoard(column.BoardId, dto.Name, id);

            // If a column with the same name already exists (excluding the current column), throw an invalid operation exception
            if (duplicateName)
            {
                throw new InvalidOperationException($"A column with the given name - {dto.Name} - already exists");
            }

            // Update the column's name
            column.Name = dto.Name;

            // Save the updated column to the repository
            var updated = await columnRepository.Update(column);

            // Log the update of the column
            logger.LogInformation("Column {Id} updated by user {UserId}", id, userId);

            // Map the updated column entity to a response DTO and return it
            return mapper.Map<ColumnResponseDto>(updated);
        }

        // The DeleteColumn method deletes an existing column, it checks if the user is an admin of the board before deleting it.
        public async Task DeleteColumn(Guid id, string userId)
        {
            // Load the column to check permissions and existing data
            var column = await columnRepository.GetById(id)
                ?? throw new KeyNotFoundException($"Column with Id {id} not found.");

            // Check if the user is an admin of the board
            var membership = await boardMemberRepository.GetByBoardAndUser(column.BoardId, userId);

            // Only Admins can delete columns
            if (membership == null || membership.Role != "Admin")
            {
                throw new UnauthorizedAccessException("Only board Admins can delete columns.");
            }

            // Delete the column from the repository
            await columnRepository.Delete(id);

            // Log the deletion of the column
            logger.LogInformation("Column {Id} deleted by user {UserId}", id, userId);
        }
    }
}
