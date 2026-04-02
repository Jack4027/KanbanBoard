using KanbanBoard.Domain.Interfaces.Repository;
using KanbanBoard.Domain.Entities;
using KanbanBoard.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace KanbanBoard.Infrastructure.Repository
{
    //All database operations associated ith cards such as creating, retrieving, updating, deleting and moving cards between columns.
    public class CardRepository(KanbanDbContext context) : ICardRepository
    {
        //Add a new card to the database, and save the changes, returning the added card with its generated Id and other properties.
        public async Task<Card> Add(Card card)
        {
            context.Cards.Add(card);
            await context.SaveChangesAsync();
            return card;
        }

        //Retrieve a card from the database based on its unique identifier (Id). If a card with the specified Id exists, it returns the card; otherwise, it returns null.
        public async Task<Card?> GetById(Guid id)
        {
            return await context.Cards
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        //Update an existing card in the database. It takes a Card object as input, updates the corresponding record in the database, saves the changes, and returns the updated card.
        public async Task<Card> Update(Card card)
        {
            context.Cards.Update(card);
            await context.SaveChangesAsync();
            return card;
        }

        //Delete a card from the database based on its unique identifier (Id). It returns true if the deletion was successful (i.e., at least one row was affected), and false otherwise.
        public async Task<bool> Delete(Guid id)
        {
            var rowsAffected = await context.Cards
                .Where(c => c.Id == id)
                .ExecuteDeleteAsync();
            return rowsAffected > 0;
        }

        //Move a card to a different column by updating its ColumnId property. It takes the card's unique identifier (cardId) and the target column's unique identifier (targetColumnId) as parameters.
        //If the card exists, it updates the ColumnId, saves the changes, and returns the updated card; if the card does not exist, it returns null.
        public async Task<Card?> MoveCard(Guid cardId, Guid targetColumnId)
        {
            var card = await context.Cards.FindAsync(cardId);

            //return null if the card with the specified Id does not exist in the database, indicating that the move operation cannot be performed.
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
