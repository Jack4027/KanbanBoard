using KanbanBoard.Domain.Interfaces.Repository;
using KanbanBoard.Domain.Entities;
using KanbanBoard.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace KanbanBoard.Infrastructure.Repository
{
    public class ColumnRepository(KanbanDbContext context) : IColumnRepository
    {
        public async Task<Column> Add(Column column)
        {
            context.Columns.Add(column);
            await context.SaveChangesAsync();
            return column;
        }

        public async Task<Column?> GetById(Guid id)
        {
            return await context.Columns
                .Include(c => c.Cards)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Column> Update(Column column)
        {
            context.Columns.Update(column);
            await context.SaveChangesAsync();
            return column;
        }

        public async Task<bool> Delete(Guid id)
        {
            var rowsAffected = await context.Columns
                .Where(c => c.Id == id)
                .ExecuteDeleteAsync();
            return rowsAffected > 0;
        }


        public async Task<bool> ExistsWithNameOnBoard(Guid boardId, string name, Guid? excludeId = null)
        {
            return await context.Columns
                .AnyAsync(c => c.BoardId == boardId &&
                               c.Name.ToLower() == name.ToLower() &&
                               (excludeId == null || c.Id != excludeId));
        }
    }
}
