using System;
using System.Collections.Generic;
using System.Text;

namespace KanbanBoard.Application.DTOs.Auth.Responses
{
    //Response for login and register operations, sent back for successful requests
    public record AuthResponseDto(
        string Token,
        string Email,
        string FullName);
}
