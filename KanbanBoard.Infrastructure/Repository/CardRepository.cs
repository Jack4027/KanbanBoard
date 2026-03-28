using KanbanBoard.Domain.Interfaces.Repository;
using KanbanBoard.Domain.Entities;
using KanbanBoard.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace KanbanBoard.Infrastructure.Repository
{
    public class CardRepository(KanbanDbContext context) : ICardRepository
    {
        public async Task<Card> Add(Card card)
        {
            context.Cards.Add(card);
            await context.SaveChangesAsync();
            return card;
        }

        public async Task<Card?> GetById(Guid id)
        {
            return await context.Cards
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Card> Update(Card card)
        {
            context.Cards.Update(card);
            await context.SaveChangesAsync();
            return card;
        }

        public async Task<bool> Delete(Guid id)
        {
            var rowsAffected = await context.Cards
                .Where(c => c.Id == id)
                .ExecuteDeleteAsync();
            return rowsAffected > 0;
        }

        public async Task<Card?> MoveCard(Guid cardId, Guid targetColumnId)
        {
            var card = await context.Cards.FindAsync(cardId);
            
            if (card == null)
            {
                return card;
            }

            card.ColumnId = targetColumnId;
            await context.SaveChangesAsync();
            return card;
        }
    }
}
