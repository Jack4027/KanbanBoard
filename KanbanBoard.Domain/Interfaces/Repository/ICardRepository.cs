using KanbanBoard.Domain.Entities;

namespace KanbanBoard.Domain.Interfaces.Repository
{
    public interface ICardRepository
    {
        Task<Card> Add(Card card);
        Task<Card?> GetById(Guid id);
        Task<Card> Update(Card card);
        Task<bool> Delete(Guid id);
        Task<Card> MoveCard(Guid cardId, Guid targetColumnId);
    }
}
