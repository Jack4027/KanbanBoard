using System;
using System.Collections.Generic;
using System.Text;

namespace KanbanBoard.Application.DTOs.Auth.Requests
{
    public record RegisterDto(
        string FirstName,
        string LastName,
        string Email,
        string Password);

    public record LoginDto(string Email, string Password);


    public record AuthUserDto(
    string Id,
    string Email,
    string FirstName,
    string LastName);
}
