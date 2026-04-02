using KanbanBoard.Application.DTOs.Requests.Column;
using KanbanBoard.Application.DTOs.Responses;

namespace KanbanBoard.Application.Interfaces.Services
{
    //Interface that defines the methods related to the columns, such as creating, updating and deleting columns.
    public interface IColumnService
    {
        Task<ColumnResponseDto> CreateColumn(Guid boardId, CreateColumnDto dto, string userId);
        Task<ColumnResponseDto> UpdateColumn(Guid id, UpdateColumnDto dto, string userId);
        Task DeleteColumn(Guid id, string userId);
    }
}
