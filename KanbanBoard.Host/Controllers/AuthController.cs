using KanbanBoard.Application.DTOs.Auth.Requests;
using KanbanBoard.Application.Interfaces.Identity;
using Microsoft.AspNetCore.Mvc;

namespace KanbanBoard.Host.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController(IAuthService authService) : ControllerBase
    {
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            var response = await authService.Register(dto);
            return Ok(response);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var response = await authService.Login(dto);
            return Ok(response);
        }
    }
}
