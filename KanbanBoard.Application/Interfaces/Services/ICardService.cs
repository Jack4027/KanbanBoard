using KanbanBoard.Application.DTOs.Requests;
using KanbanBoard.Application.DTOs.Responses;

namespace KanbanBoard.Application.Interfaces.Services
{
    public interface ICardService
    {
        Task<CardResponseDto> CreateCard(Guid columnId, CreateCardDto dto, string userId);
        Task<CardResponseDto> UpdateCard(Guid id, UpdateCardDto dto, string userId);
        Task<CardResponseDto> MoveCard(Guid cardId, MoveCardDto dto, string userId);
        Task DeleteCard(Guid id, string userId);
    }
}
