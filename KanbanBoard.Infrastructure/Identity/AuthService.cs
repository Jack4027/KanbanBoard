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
    // The AuthService class implements the IAuthService interface and provides methods for user registration and login,
    // utilizing ASP.NET Core Identity for user management and authentication, AutoMapper for mapping between domain entities and DTOs,
    // and a custom token service for generating JWT tokens upon successful authentication.
    public class AuthService(
        UserManager<AppUserIdentity> userManager,
        ITokenService tokenService,
        IMapper mapper,
        ILogger<AuthService> logger) : IAuthService
    {
        // The Register method handles user registration by checking for existing users, creating a new user with the provided information, and generating a JWT token for the newly registered user.
        // It also logs the registration event and returns an AuthResponseDto containing the token and user information.
        public async Task<AuthResponseDto> Register(RegisterDto dto)
        {
            // Check if a user with the provided email already exists
            var existingUser = await userManager.FindByEmailAsync(dto.Email);

            // If a user with the email already exists, throw an exception to prevent duplicate registrations
            if (existingUser != null)
            {
                throw new InvalidOperationException($"User with email '{dto.Email}' already exists.");
            }

            // Create a new AppUserIdentity instance with the provided registration information
            var user = new AppUserIdentity
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                UserName = dto.Email
            };

            // Attempt to create the user in the database with the specified password
            var result = await userManager.CreateAsync(user, dto.Password);

            // If the user creation failed, gather the error messages and throw an exception with the details
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Registration failed: {errors}");
            }

            // Map the created user to an AuthUserDto, then use this model to generate a JWT token for the authenticated user
            var authUser = mapper.Map<AuthUserDto>(user);
            var token = tokenService.GenerateToken(authUser);

            // Log the successful registration event with the user's email
            logger.LogInformation("User {Email} registered", user.Email);

            // Return an AuthResponseDto containing the generated token and user information
            return new AuthResponseDto(token, user.Email!, $"{user.FirstName} {user.LastName}");
        }

        //Login method handles user authentication by verifying the provided email and password against the stored user data.
        public async Task<AuthResponseDto> Login(LoginDto dto)
        {
            // Attempt to find the user by email. If the user does not exist, throw an exception indicating invalid credentials.
            var user = await userManager.FindByEmailAsync(dto.Email)
                ?? throw new KeyNotFoundException("Invalid email or password.");

            // Check if the provided password is correct for the found user. If the password is invalid, throw an exception indicating invalid credentials.
            var validPassword = await userManager.CheckPasswordAsync(user, dto.Password);
            if (!validPassword)
            {
                throw new KeyNotFoundException("Invalid email or password.");
            }

            // If the user is found and the password is valid, map the user to an AuthUserDto and generate a JWT token for the authenticated user.
            var authUser = mapper.Map<AuthUserDto>(user);
            var token = tokenService.GenerateToken(authUser);

            //log the successful login event with the user's email
            logger.LogInformation("User {Email} logged in", user.Email);

            // Return an AuthResponseDto containing the generated token and user information
            return new AuthResponseDto(token, user.Email!, $"{user.FirstName} {user.LastName}");
        }
    }
}
