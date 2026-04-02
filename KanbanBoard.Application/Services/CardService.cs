using AutoMapper;
using KanbanBoard.Application.DTOs.Requests.Card;
using KanbanBoard.Application.DTOs.Responses;
using KanbanBoard.Application.Interfaces.Hubs;
using KanbanBoard.Application.Interfaces.Services;
using KanbanBoard.Domain.Entities;
using KanbanBoard.Domain.Interfaces.Repository;
using Microsoft.Extensions.Logging;


namespace KanbanBoard.Application.Services
{
    //Service that implements the ICardService interface, it contains the business logic for creating, updating, moving and deleting cards in the kanban board.
    // It uses the repositories to access the data and the notification service to send notifications to the clients when a card is moved, created or deleted.
    public class CardService(
        ICardRepository cardRepository,
        IColumnRepository columnRepository,
        IBoardMemberRepository boardMemberRepository,
        IKanbanNotificationService notificationService,
        IMapper mapper,
        ILogger<CardService> logger) : ICardService
    {
        // Method to create a card in a column, it checks if the column exists and if the user has access to the board before creating the card, then it sends a notification to the clients about the new card.
        public async Task<CardResponseDto> CreateCard(Guid columnId, CreateCardDto dto, string userId)
        {
            // Check if the column exists
            var column = await columnRepository.GetById(columnId)
                ?? throw new KeyNotFoundException($"Column with Id {columnId} not found.");

            // Check if the user has access to the board
            var membership = await boardMemberRepository.GetByBoardAndUser(column.BoardId, userId);
            if (membership == null)
                throw new UnauthorizedAccessException("You do not have access to this board.");

            //  Create the card entity
            var card = new Card
            {
                Id = Guid.NewGuid(),
                ColumnId = columnId,
                Title = dto.Title,
                Description = dto.Description
            };

            // Save the card to the database
            var saved = await cardRepository.Add(card);

            // Log the creation of the card
            logger.LogInformation("Card {Title} created in column {ColumnId}", card.Title, columnId);

            // Map the saved card to a response DTO
            var response = mapper.Map<CardResponseDto>(saved);

            // Send a notification to the clients about the new card
            await notificationService.CardCreated(column.BoardId.ToString(), response);

            return response;
        }

        // Method to update a card, it checks if the card exists and if the user has access to the board before updating the card, then it sends a notification to the clients about the updated card.
        public async Task<CardResponseDto> UpdateCard(Guid id, UpdateCardDto dto, string userId)
        {
            // Check if the card exists
            var card = await cardRepository.GetById(id)
                ?? throw new KeyNotFoundException($"Card with Id {id} not found.");

            // Check if the user has access to the board
            var column = await columnRepository.GetById(card.ColumnId)
                ?? throw new KeyNotFoundException($"Column not found.");

            // Check if the user is a member of the board
            var membership = await boardMemberRepository.GetByBoardAndUser(column.BoardId, userId);

            // If the user is not a member of the board, throw an UnauthorizedAccessException
            if (membership == null)
            {
                throw new UnauthorizedAccessException("You do not have access to this board.");
            }

            // Update the card properties
            card.Title = dto.Title;
            card.Description = dto.Description;

            // Save the updated card to the database
            var updated = await cardRepository.Update(card);

            // Log the update of the card
            logger.LogInformation("Card {Id} updated by user {UserId}", id, userId);

            // Map the updated card to a response DTO
            return mapper.Map<CardResponseDto>(updated);
        }

        // Method to move a card from one column to another, it checks if the card exists,
        // if the target column exists and if the user has access to the board before moving the card,
        // then it sends a notification to the clients about the moved card.
        public async Task<CardResponseDto> MoveCard(Guid cardId, MoveCardDto dto, string userId)
        {
            // Check if the card exists
            var card = await cardRepository.GetById(cardId)
                ?? throw new KeyNotFoundException($"Card with Id {cardId} not found.");

            // Check if the target column is the same as the current column, if so throw invalid operation exception
            if (dto.TargetColumnId == card.ColumnId)
            {
                throw new InvalidOperationException("Card is already in the target column");
            }

            // Check if the source and target columns exist
            var sourceColumn = await columnRepository.GetById(card.ColumnId)
                ?? throw new KeyNotFoundException("Source column not found.");

            // Check if the target column exists
            var targetColumn = await columnRepository.GetById(dto.TargetColumnId)
                ?? throw new KeyNotFoundException("Target column not found.");

            // Check if the source and target columns belong to the same board
            if (sourceColumn.BoardId != targetColumn.BoardId)
            {
                throw new InvalidOperationException("Cannot move a card to a column on a different board.");
            }

            // Check if the user has access to the board
            var membership = await boardMemberRepository.GetByBoardAndUser(sourceColumn.BoardId, userId);

            // If the user is not a member of the board, throw an UnauthorizedAccessException
            if (membership == null)
            {
                throw new UnauthorizedAccessException("You do not have access to this board.");
            }

            // Call the repo to move the card to the target column
            var moved =  await cardRepository.MoveCard(cardId, dto.TargetColumnId);

            //Log the movement of the card
            logger.LogInformation("Card {CardId} moved to column {ColumnId} by user {UserId}",
                cardId, dto.TargetColumnId, userId);

            // Map the moved card to a response DTO
            var response = mapper.Map<CardResponseDto>(moved!);

            //Notify clients about the moved card
            await notificationService.CardMoved(sourceColumn.BoardId.ToString(), response);

            //Return the response DTO
            return response;
        }

        // Method to delete a card, it checks if the card exists and if the user has access to the board before deleting the card, then it sends a notification to the clients about the deleted card.
        public async Task DeleteCard(Guid id, string userId)
        {
            // Check if the card exists
            var card = await cardRepository.GetById(id)
                ?? throw new KeyNotFoundException($"Card with Id {id} not found.");

            // Check if the user has access to the board
            var column = await columnRepository.GetById(card.ColumnId)
                ?? throw new KeyNotFoundException("Column not found.");

            // Check if the user is a member of the board
            var membership = await boardMemberRepository.GetByBoardAndUser(column.BoardId, userId);

            // If the user is not a member of the board, throw an UnauthorizedAccessException
            if (membership == null)
            {
                throw new UnauthorizedAccessException("You do not have access to this board.");
            }

            // Call the repo to delete the card
            await cardRepository.Delete(id);

            // Notify clients about the deleted card
            await notificationService.CardDeleted(column.BoardId.ToString(), id, column.Id);

            //  Log the deletion of the card
            logger.LogInformation("Card {Id} deleted by user {UserId}", id, userId);
        }
    }
}
