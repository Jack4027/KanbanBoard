using KanbanBoard.Application.DTOs.Auth.Requests;
using KanbanBoard.Application.Interfaces.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace KanbanBoard.Host.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController(IAuthService authService) : ControllerBase
    {
        [HttpPost("register")]
        [EnableRateLimiting("auth")]

        public async Task<IActionResult> Register(RegisterDto dto)
        {
            var response = await authService.Register(dto);
            return Ok(response);
        }

        [HttpPost("login")]
        [EnableRateLimiting("auth")]

        public async Task<IActionResult> Login(LoginDto dto)
        {
            var response = await authService.Login(dto);
            return Ok(response);
        }
    }
}
