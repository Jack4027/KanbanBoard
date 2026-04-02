using KanbanBoard.Application.DTOs.Auth.Requests;
using KanbanBoard.Application.Interfaces.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace KanbanBoard.Host.Controllers
{
    //Controller responsible for handling authentication requests, it has two endpoints, one for registering a new user and another for logging in an existing user.
    //Both endpoints are rate limited to prevent abuse.
    [ApiController]
    [Route("api/auth")]
    public class AuthController(IAuthService authService) : ControllerBase
    {
        //Endpoint for registering a new user,
        //it receives a RegisterDto object in the request body and returns a response with the user details and a JWT token.
        [HttpPost("register")]
        [EnableRateLimiting("auth")]

        public async Task<IActionResult> Register(RegisterDto dto)
        {
            var response = await authService.Register(dto);
            return Ok(response);
        }

        //Endpoint for logging in an existing user,
        //it receives a LoginDto object in the request body and returns a response with the user details and a JWT token.
        [HttpPost("login")]
        [EnableRateLimiting("auth")]

        public async Task<IActionResult> Login(LoginDto dto)
        {
            var response = await authService.Login(dto);
            return Ok(response);
        }
    }
}
