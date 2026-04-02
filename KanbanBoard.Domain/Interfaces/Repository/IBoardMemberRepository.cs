using KanbanBoard.Domain.Entities;

namespace KanbanBoard.Domain.Interfaces.Repository
{
    ///Interface used to define the methods that will be implemented in the BoardMemberRepository,
    ///which is used to interact with the database to perform CRUD operations on the BoardMember entity,
    ///such as getting a board member by board and user,
    ///getting all board members by board id and deleting a board member from a board.
    public interface IBoardMemberRepository
    {
        Task<BoardMember?> GetByBoardAndUser(Guid boardId, string userId);
        Task<IEnumerable<BoardMember>> GetByBoardId(Guid boardId);
        Task<bool> Delete(Guid boardId, string userId);
    }
}
