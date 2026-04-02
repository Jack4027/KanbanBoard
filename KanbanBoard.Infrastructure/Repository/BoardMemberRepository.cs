using KanbanBoard.Domain.Entities;
using KanbanBoard.Domain.Interfaces.Repository;
using KanbanBoard.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace KanbanBoard.Infrastructure.Repository
{
    //All database operations associated with board members
    public class BoardMemberRepository(KanbanDbContext context) : IBoardMemberRepository
    {
        //Get board using composite key of boardId and userId
        public async Task<BoardMember?> GetByBoardAndUser(Guid boardId, string userId)
        {
            return await context.BoardMembers
                .FirstOrDefaultAsync(m => m.BoardId == boardId && m.UserId == userId);
        }

        //Get all members associated with a specific board using boardId, returns an empty list if no members are found
        public async Task<IEnumerable<BoardMember>> GetByBoardId(Guid boardId)
        {
            return await context.BoardMembers
                .Where(m => m.BoardId == boardId)
                .ToListAsync();
        }

        //Delete a board member using composite key of boardId and userId, returns true if a record was deleted, false otherwise
        public async Task<bool> Delete(Guid boardId, string userId)
        {
            var rowsAffected = await context.BoardMembers
                .Where(m => m.BoardId == boardId && m.UserId == userId)
                .ExecuteDeleteAsync();
            return rowsAffected > 0;
        }
    }
}
