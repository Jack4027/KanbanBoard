using KanbanBoard.Domain.Entities;

namespace KanbanBoard.Domain.Interfaces.Repository
{
    //Interface that defines the methods that will be implemented by the ColumnRepository,
    //which is responsible for handling the data access for the Column entity.
    public interface IColumnRepository
    {
        Task<Column> Add(Column column);
        Task<Column?> GetById(Guid id);
        Task<Column> Update(Column column);
        Task<bool> Delete(Guid id);
        Task<bool> ExistsWithNameOnBoard(Guid boardId, string name, Guid? excludeId = null);
    }
}
