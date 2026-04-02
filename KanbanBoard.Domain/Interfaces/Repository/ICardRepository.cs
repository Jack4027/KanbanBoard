using KanbanBoard.Domain.Entities;

namespace KanbanBoard.Domain.Interfaces.Repository
{

    //Interface used to define the methods that will be implemented in the CardRepository class,
    //which will be responsible for handling the data access logic for the Card entity,
    //such as adding, updating, deleting and moving cards between columns.
    public interface ICardRepository
    {
        Task<Card> Add(Card card);
        Task<Card?> GetById(Guid id);
        Task<Card> Update(Card card);
        Task<bool> Delete(Guid id);
        Task<Card> MoveCard(Guid cardId, Guid targetColumnId);
    }
}
