using AutoMapper;
using KanbanBoard.Application.DTOs.Requests;
using KanbanBoard.Application.DTOs.Responses;
using KanbanBoard.Application.Interfaces.Hubs;
using KanbanBoard.Application.Interfaces.Services;
using KanbanBoard.Domain.Entities;
using KanbanBoard.Domain.Interfaces.Repository;
using Microsoft.Extensions.Logging;


namespace KanbanBoard.Application.Services
{
    public class CardService(
        ICardRepository cardRepository,
        IColumnRepository columnRepository,
        IBoardMemberRepository boardMemberRepository,
        IKanbanNotificationService notificationService,
        IMapper mapper,
        ILogger<CardService> logger) : ICardService
    {
        public async Task<CardResponseDto> CreateCard(Guid columnId, CreateCardDto dto, string userId)
        {
            var column = await columnRepository.GetById(columnId)
                ?? throw new KeyNotFoundException($"Column with Id {columnId} not found.");

            var membership = await boardMemberRepository.GetByBoardAndUser(column.BoardId, userId);
            if (membership == null)
                throw new UnauthorizedAccessException("You do not have access to this board.");

            var card = new Card
            {
                Id = Guid.NewGuid(),
                ColumnId = columnId,
                Title = dto.Title,
                Description = dto.Description
            };

            var saved = await cardRepository.Add(card);

            logger.LogInformation("Card {Title} created in column {ColumnId}", card.Title, columnId);

            var response = mapper.Map<CardResponseDto>(saved);

            await notificationService.CardCreated(column.BoardId.ToString(), response);

            return response;
        }

        public async Task<CardResponseDto> UpdateCard(Guid id, UpdateCardDto dto, string userId)
        {
            var card = await cardRepository.GetById(id)
                ?? throw new KeyNotFoundException($"Card with Id {id} not found.");

            var column = await columnRepository.GetById(card.ColumnId)
                ?? throw new KeyNotFoundException($"Column not found.");

            var membership = await boardMemberRepository.GetByBoardAndUser(column.BoardId, userId);
            if (membership == null)
                throw new UnauthorizedAccessException("You do not have access to this board.");

            card.Title = dto.Title;
            card.Description = dto.Description;

            var updated = await cardRepository.Update(card);

            logger.LogInformation("Card {Id} updated by user {UserId}", id, userId);

            return mapper.Map<CardResponseDto>(updated);
        }

        public async Task<CardResponseDto> MoveCard(Guid cardId, MoveCardDto dto, string userId)
        {
            var card = await cardRepository.GetById(cardId)
                ?? throw new KeyNotFoundException($"Card with Id {cardId} not found.");

            if(dto.TargetColumnId == card.ColumnId)
            {
                throw new InvalidOperationException("Card is already in the target column");
            }

            var sourceColumn = await columnRepository.GetById(card.ColumnId)
                ?? throw new KeyNotFoundException("Source column not found.");

            var targetColumn = await columnRepository.GetById(dto.TargetColumnId)
                ?? throw new KeyNotFoundException("Target column not found.");

            if (sourceColumn.BoardId != targetColumn.BoardId)
                throw new InvalidOperationException("Cannot move a card to a column on a different board.");

            var membership = await boardMemberRepository.GetByBoardAndUser(sourceColumn.BoardId, userId);
            if (membership == null)
                throw new UnauthorizedAccessException("You do not have access to this board.");

           var moved =  await cardRepository.MoveCard(cardId, dto.TargetColumnId);

            logger.LogInformation("Card {CardId} moved to column {ColumnId} by user {UserId}",
                cardId, dto.TargetColumnId, userId);

            var response = mapper.Map<CardResponseDto>(moved!);

            await notificationService.CardMoved(sourceColumn.BoardId.ToString(), response);

            return response;
        }

        public async Task DeleteCard(Guid id, string userId)
        {
            var card = await cardRepository.GetById(id)
                ?? throw new KeyNotFoundException($"Card with Id {id} not found.");

            var column = await columnRepository.GetById(card.ColumnId)
                ?? throw new KeyNotFoundException("Column not found.");

            var membership = await boardMemberRepository.GetByBoardAndUser(column.BoardId, userId);
            if (membership == null)
                throw new UnauthorizedAccessException("You do not have access to this board.");

            await cardRepository.Delete(id);

            await notificationService.CardDeleted(column.BoardId.ToString(), id, column.Id);

            logger.LogInformation("Card {Id} deleted by user {UserId}", id, userId);
        }
    }
}
