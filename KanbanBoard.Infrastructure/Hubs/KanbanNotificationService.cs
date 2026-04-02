using KanbanBoard.Application.DTOs.Responses;
using KanbanBoard.Application.Interfaces.Hubs;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Text;

namespace KanbanBoard.Infrastructure.Hubs
{
    //Kanban notification service implements the IKanbanNotificationService interface and is responsible for sending real-time notifications to clients connected to the KanbanHub when certain events occur, such as when a card is moved, created, or deleted on a board.
    //It uses the IHubContext<KanbanHub> to send messages to specific groups of clients based on the board ID, allowing for targeted notifications to users who are currently viewing or interacting with that board.
    //The kanban hub is passed as context because the notification service needs to interact with the hub to send messages to clients, through this context it can access the hub's functionality to manage group memberships and send notifications to clients in real-time.
    public class KanbanNotificationService(IHubContext<KanbanHub> hubContext)
        : IKanbanNotificationService
    {
        public async Task CardMoved(string boardId, CardResponseDto card)
        {
            await hubContext.Clients.Group(boardId)
                .SendAsync("CardMoved", card);
        }

        public async Task CardCreated(string boardId, CardResponseDto card)
        {
            await hubContext.Clients.Group(boardId)
                .SendAsync("CardCreated", card);
        }

        public async Task CardDeleted(string boardId, Guid cardId, Guid columnId)
        {
            await hubContext.Clients.Group(boardId)
                .SendAsync("CardDeleted", cardId, columnId);
        }
    }
}
