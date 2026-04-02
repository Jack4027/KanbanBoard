using System;
using System.Collections.Generic;
using System.Text;

namespace KanbanBoard.Application.DTOs.Auth.Requests
{
    //Passed when a user registers, record types used in the request objects in order to maintain immutability
    public record RegisterDto(
        string FirstName,
        string LastName,
        string Email,
        string Password);
}
