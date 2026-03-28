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
    [Authorize]
    public class KanbanHub : Hub
    {
        public async Task JoinBoard(string boardId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, boardId);
        }

        public async Task LeaveBoard(string boardId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, boardId);
        }
    }

//---

//** What this does**

//`KanbanHub` is the SignalR hub — the server side endpoint that manages WebSocket connections.

//`JoinBoard` — when a user opens a board in Angular they call this method.SignalR adds their connection to a group named after the board Id. Every user viewing the same board is in the same group.

//`LeaveBoard` — when a user navigates away they call this to leave the group. Their connection is removed and they stop receiving updates for that board.

//`[Authorize]` — only authenticated users can connect to the hub. The JWT token is validated on connection.

//---

//**Groups are the key concept**

//Think of a group as a broadcast channel:
//```
//Board A group:
//  → John's connection
//  → Sarah's connection
//  → Mike's connection

//Board B group:
//  → Lisa's connection
}
