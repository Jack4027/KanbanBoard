using KanbanBoard.Domain.Interfaces.Repository;
using KanbanBoard.Domain.Entities;
using KanbanBoard.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace KanbanBoard.Infrastructure.Repository
{
    //All database operations associated with columns such as creating, retrieving, updating, deleting and checking for the existence of columns with a specific name on a board.
    public class ColumnRepository(KanbanDbContext context) : IColumnRepository
    {
        //Add a new column to the database and save changes.
        public async Task<Column> Add(Column column)
        {
            context.Columns.Add(column);
            await context.SaveChangesAsync();
            return column;
        }

        //Retrieve a column by its unique identifier, including its associated cards.
        public async Task<Column?> GetById(Guid id)
        {
            return await context.Columns
                .Include(c => c.Cards)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        //Update an existing column's details in the database and save changes.
        public async Task<Column> Update(Column column)
        {
            context.Columns.Update(column);
            await context.SaveChangesAsync();
            return column;
        }

        //Delete a column from the database by its unique identifier and return whether the operation was successful.
        public async Task<bool> Delete(Guid id)
        {
            var rowsAffected = await context.Columns
                .Where(c => c.Id == id)
                .ExecuteDeleteAsync();
            return rowsAffected > 0;
        }

        //Check if a column with the specified name already exists on a board, excluding a specific column ID if provided (useful for validating uniqueness when updating a column's name).
        public async Task<bool> ExistsWithNameOnBoard(Guid boardId, string name, Guid? excludeId = null)
        {
            return await context.Columns
                .AnyAsync(c => c.BoardId == boardId &&
                               c.Name.ToLower() == name.ToLower() &&
                               (excludeId == null || c.Id != excludeId));
        }
    }
}
