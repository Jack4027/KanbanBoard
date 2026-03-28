using KanbanBoard.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace KanbanBoard.Domain.Interfaces.Repository
{
    public interface IBoardRepository
    {
        Task<Board> Add(Board board);
        Task<Board?> GetById(Guid id);
        Task<IEnumerable<Board>> GetByUserId(string userId);
        Task<Board> Update(Board board);
        Task<bool> Delete(Guid id);
    }
}
