using KanbanBoard.Domain.Interfaces.Repository;
using KanbanBoard.Domain.Entities;
using KanbanBoard.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace KanbanBoard.Infrastructure.Repository
{
    public class BoardRepository(KanbanDbContext context) : IBoardRepository
    {
        public async Task<Board> Add(Board board)
        {
            context.Boards.Add(board);
            await context.SaveChangesAsync();
            return board;
        }

        public async Task<Board?> GetById(Guid id)
        {
            return await context.Boards
                .Include(b => b.Columns.OrderBy(c => c.Position))
                    .ThenInclude(c => c.Cards.OrderBy(card => card.CreatedAt))
                .Include(b => b.Members)
                .FirstOrDefaultAsync(b => b.Id == id);
        }


        public async Task<(IEnumerable<Board> Items, int TotalCount)> GetByUserId(string userId, int page, int pageSize)
        {
            var query = context.Boards
                .Include(b => b.Columns.OrderBy(c => c.Position))
                .Include(b => b.Members)
                .Where(b => b.CreatedBy == userId ||
                            b.Members.Any(m => m.UserId == userId));

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(b => b.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<Board> Update(Board board)
        {
            context.Boards.Update(board);
            await context.SaveChangesAsync();
            return board;
        }

        public async Task<bool> Delete(Guid id)
        {
            var rowsAffected = await context.Boards
                .Where(b => b.Id == id)
                .ExecuteDeleteAsync();
            return rowsAffected > 0;
        }

        public async Task<bool> ExistsWithNameOnBoard(Guid boardId, string name, Guid? excludeId = null)
        {
            return await context.Columns
            .AnyAsync(c => c.BoardId == boardId &&
                   c.Name.ToLower() == name.ToLower() &&
                   c.Id != excludeId);
        }
    }
}
