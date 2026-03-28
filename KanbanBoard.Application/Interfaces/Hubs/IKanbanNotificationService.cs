using KanbanBoard.Application.DTOs.Responses;
using System;
using System.Collections.Generic;
using System.Text;

namespace KanbanBoard.Application.Interfaces.Hubs
{
    public interface IKanbanNotificationService
    {
        Task CardMoved(string boardId, CardResponseDto card);
        Task CardCreated(string boardId, CardResponseDto card);
        Task CardDeleted(string boardId, Guid cardId, Guid columnId);
    }
}
