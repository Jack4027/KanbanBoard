using KanbanBoard.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace KanbanBoard.Domain.Interfaces.Repository
{
    /// This interface defines the contract for the Board repository,
    /// which is responsible for handling all the data access operations related to the Board entity,
    /// such as adding, retrieving, updating and deleting boards from the database.
    public interface IBoardRepository
    {
        Task<Board> Add(Board board);
        Task<Board?> GetById(Guid id);
        Task<(IEnumerable<Board> Items, int TotalCount)> GetByUserId(string userId, int page, int pageSize);
        Task<Board> Update(Board board);
        Task<bool> Delete(Guid id);
    }
}
