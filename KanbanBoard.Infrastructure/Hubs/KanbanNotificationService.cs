using KanbanBoard.Application.DTOs.Responses;
using KanbanBoard.Application.Interfaces.Hubs;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Text;

namespace KanbanBoard.Infrastructure.Hubs
{
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
