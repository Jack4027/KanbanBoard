using KanbanBoard.Domain.Entities;
using KanbanBoard.Domain.Interfaces.Repository;
using KanbanBoard.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace KanbanBoard.Infrastructure.Repository
{
    public class BoardMemberRepository(KanbanDbContext context) : IBoardMemberRepository
    {

        public async Task<BoardMember?> GetByBoardAndUser(Guid boardId, string userId)
        {
            return await context.BoardMembers
                .FirstOrDefaultAsync(m => m.BoardId == boardId && m.UserId == userId);
        }

        public async Task<IEnumerable<BoardMember>> GetByBoardId(Guid boardId)
        {
            return await context.BoardMembers
                .Where(m => m.BoardId == boardId)
                .ToListAsync();
        }

        public async Task<bool> Delete(Guid boardId, string userId)
        {
            var rowsAffected = await context.BoardMembers
                .Where(m => m.BoardId == boardId && m.UserId == userId)
                .ExecuteDeleteAsync();
            return rowsAffected > 0;
        }
    }
}
