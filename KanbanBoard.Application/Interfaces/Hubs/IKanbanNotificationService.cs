using KanbanBoard.Application.DTOs.Responses;
using System;
using System.Collections.Generic;
using System.Text;

namespace KanbanBoard.Application.Interfaces.Hubs
{
    //Interface used to inject the NotificationHub in the services.
    //It defines the methods that will be called in the hub to send notifications to the clients when a card is moved, created or deleted.
    public interface IKanbanNotificationService
    {
        Task CardMoved(string boardId, CardResponseDto card);
        Task CardCreated(string boardId, CardResponseDto card);
        Task CardDeleted(string boardId, Guid cardId, Guid columnId);
    }
}
