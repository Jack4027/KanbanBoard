using KanbanBoard.Domain.Entities;

namespace KanbanBoard.Domain.Interfaces.Repository
{
    public interface IBoardMemberRepository
    {
        Task<BoardMember?> GetByBoardAndUser(Guid boardId, string userId);
        Task<IEnumerable<BoardMember>> GetByBoardId(Guid boardId);
        Task<bool> Delete(Guid boardId, string userId);
    }
}
