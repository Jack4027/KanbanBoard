using KanbanBoard.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Metrics;
using System.Drawing;
using System.Net.WebSockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Channels;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace KanbanBoard.Infrastructure.Hubs
{
    // The KanbanHub class is a SignalR hub that facilitates real-time communication between the server and clients for the Kanban board application.
    [Authorize]
    public class KanbanHub : Hub
    {
        //Join board allows client to join a board and its associated group in SignalR
        public async Task JoinBoard(string boardId)
        {
            //Connection Id is a unique identifier for each client connection to the SignalR hub, it is used to manage group memberships and send targeted notifications to specific clients based on their connection ID.
            await Groups.AddToGroupAsync(Context.ConnectionId, boardId);
        }

        //Leave board removes a client from a board and its associated group, they will cease to receive notifications related to that board
        public async Task LeaveBoard(string boardId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, boardId);
        }
    }

}
