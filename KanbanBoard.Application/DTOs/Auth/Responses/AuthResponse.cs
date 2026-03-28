using System;
using System.Collections.Generic;
using System.Text;

namespace KanbanBoard.Application.DTOs.Auth.Responses
{
    public record AuthResponseDto(
        string Token,
        string Email,
        string FullName);
}
