using KanbanBoard.Application.DTOs.Auth.Requests;
using KanbanBoard.Application.Interfaces.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace KanbanBoard.Infrastructure.Identity
{
    // The TokenService class implements the ITokenService interface and is responsible for generating JWT tokens for authenticated users.
    // It uses the configuration settings to retrieve the secret key, issuer, and audience for token generation, and creates a token that includes claims such as the user's ID, email,
    // and name, with an expiration time of 8 hours.
    public class TokenService(IConfiguration configuration) : ITokenService
    {
        public string GenerateToken(AuthUserDto user)
        {
            // Create a symmetric security key using the secret key from the configuration
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!));

            //key and the algorithm used to sign the token
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            //user claims to be included in the token, such as the user's ID, email, and name
            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}")
        };

            //Token itself, includes the issuer, audience, claims, expiration time, and signing credentials. The token is set to expire in 8 hours.
            var token = new JwtSecurityToken(
                issuer: configuration["Jwt:Issuer"],
                audience: configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(8),
                signingCredentials: credentials);

            //write the token to a string format that can be returned to the client
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
