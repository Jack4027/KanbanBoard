using KanbanBoard.Application.DTOs.Auth.Requests;
using KanbanBoard.Application.DTOs.Auth.Responses;
using System;
using System.Collections.Generic;
using System.Text;

namespace KanbanBoard.Application.Interfaces.Identity
{
    public interface IAuthService
    {
        Task<AuthResponseDto> Register(RegisterDto dto);
        Task<AuthResponseDto> Login(LoginDto dto);
    }
}
