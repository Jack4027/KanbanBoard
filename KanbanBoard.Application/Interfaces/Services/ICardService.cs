using KanbanBoard.Application.DTOs.Requests.Card;
using KanbanBoard.Application.DTOs.Responses;

namespace KanbanBoard.Application.Interfaces.Services
{
    //Interface that defines the methods related to the card operations, such as creating, updating, moving and deleting a card.
    public interface ICardService
    {
        Task<CardResponseDto> CreateCard(Guid columnId, CreateCardDto dto, string userId);
        Task<CardResponseDto> UpdateCard(Guid id, UpdateCardDto dto, string userId);
        Task<CardResponseDto> MoveCard(Guid cardId, MoveCardDto dto, string userId);
        Task DeleteCard(Guid id, string userId);
    }
}
