using AutoMapper;
using KanbanBoard.Application.DTOs.Auth.Requests;
using KanbanBoard.Application.DTOs.Auth.Responses;
using KanbanBoard.Application.Interfaces.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace KanbanBoard.Infrastructure.Identity
{
    public class AuthService(
        UserManager<AppUserIdentity> userManager,
        ITokenService tokenService,
        IMapper mapper,
        ILogger<AuthService> logger) : IAuthService
    {
        public async Task<AuthResponseDto> Register(RegisterDto dto)
        {
            var existingUser = await userManager.FindByEmailAsync(dto.Email);
            if (existingUser != null)
            {
                throw new InvalidOperationException($"User with email '{dto.Email}' already exists.");
            }

            var user = new AppUserIdentity
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                UserName = dto.Email
            };

            var result = await userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Registration failed: {errors}");
            }

            var authUser = mapper.Map<AuthUserDto>(user);
            var token = tokenService.GenerateToken(authUser);

            logger.LogInformation("User {Email} registered", user.Email);

            return new AuthResponseDto(token, user.Email!, $"{user.FirstName} {user.LastName}");
        }

        public async Task<AuthResponseDto> Login(LoginDto dto)
        {
            var user = await userManager.FindByEmailAsync(dto.Email)
                ?? throw new KeyNotFoundException("Invalid email or password.");

            var validPassword = await userManager.CheckPasswordAsync(user, dto.Password);
            if (!validPassword)
                throw new KeyNotFoundException("Invalid email or password.");

            var authUser = mapper.Map<AuthUserDto>(user);
            var token = tokenService.GenerateToken(authUser);

            logger.LogInformation("User {Email} logged in", user.Email);

            return new AuthResponseDto(token, user.Email!, $"{user.FirstName} {user.LastName}");
        }
    }
}
