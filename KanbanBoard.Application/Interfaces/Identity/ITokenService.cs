using KanbanBoard.Application.DTOs.Auth.Requests;
using System;
using System.Collections.Generic;
using System.Text;

namespace KanbanBoard.Application.Interfaces.Identity
{
    public interface ITokenService
    {
        string GenerateToken(AuthUserDto user);
    }
}
