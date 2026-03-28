using KanbanBoard.Domain.Entities;

namespace KanbanBoard.Domain.Interfaces.Repository
{
    public interface IColumnRepository
    {
        Task<Column> Add(Column column);
        Task<Column?> GetById(Guid id);
        Task<Column> Update(Column column);
        Task<bool> Delete(Guid id);
        Task<bool> ExistsWithNameOnBoard(Guid boardId, string name, Guid? excludeId = null);
    }
}
