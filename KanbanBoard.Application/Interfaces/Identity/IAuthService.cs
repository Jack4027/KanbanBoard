using KanbanBoard.Application.DTOs.Auth.Requests;
using KanbanBoard.Application.DTOs.Auth.Responses;
using System;
using System.Collections.Generic;
using System.Text;

namespace KanbanBoard.Application.Interfaces.Identity
{
    //Interface used to inject the AuthService in the controllers, it defines the methods that will be used for authentication operations such as register and login
    public interface IAuthService
    {
        Task<AuthResponseDto> Register(RegisterDto dto);
        Task<AuthResponseDto> Login(LoginDto dto);
    }
}
