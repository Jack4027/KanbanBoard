using KanbanBoard.Application.DTOs.Auth.Requests;
using System;
using System.Collections.Generic;
using System.Text;

namespace KanbanBoard.Application.Interfaces.Identity
{
    //Interface used to inject the TokenService in the controllers, it defines the method to generate a JWT token for the authenticated user,
    //it takes the user data as a parameter and returns the generated token as a string.
    public interface ITokenService
    {
        string GenerateToken(AuthUserDto user);
    }
}
