using KanbanBoard.Application.DTOs.Auth;
using KanbanBoard.Application.DTOs.Auth.Requests;
using KanbanBoard.Infrastructure.Identity;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace KanbanBoard.Test;

[TestFixture]
public class TokenServiceTests
{
    private TokenService _tokenService;

    [SetUp]
    public void Setup()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Jwt:Key", "TestSecretKeyThatIsAtLeast32CharactersLong!" },
                { "Jwt:Issuer", "KanbanBoard" },
                { "Jwt:Audience", "KanbanBoardUsers" }
            })
            .Build();

        _tokenService = new TokenService(config);
    }

    // Tests that GenerateToken returns a non empty token string
    [Test]
    public void GenerateTokenReturnsNonEmptyToken()
    {
        var user = new AuthUserDto(
            Guid.NewGuid().ToString(),
            "john@kanban.com",
            "John",
            "Doe");

        var token = _tokenService.GenerateToken(user);

        Assert.That(token, Is.Not.Null);
        Assert.That(token, Is.Not.Empty);
    }

    // Tests that GenerateToken returns a valid JWT containing correct claims
    [Test]
    public void GenerateTokenContainsCorrectClaims()
    {
        var userId = Guid.NewGuid().ToString();

        var user = new AuthUserDto(
            userId,
            "john@kanban.com",
            "John",
            "Doe");

        var token = _tokenService.GenerateToken(user);

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        Assert.That(jwt.Claims.Any(c =>
            c.Type == ClaimTypes.NameIdentifier && c.Value == userId), Is.True);

        Assert.That(jwt.Claims.Any(c =>
            c.Type == ClaimTypes.Email && c.Value == "john@kanban.com"), Is.True);

        Assert.That(jwt.Claims.Any(c =>
            c.Type == ClaimTypes.Name && c.Value == "John Doe"), Is.True);
    }

    // Tests that GenerateToken returns a token that has not expired
    [Test]
    public void GenerateTokenReturnsNonExpiredToken()
    {
        var user = new AuthUserDto(
            Guid.NewGuid().ToString(),
            "john@kanban.com",
            "John",
            "Doe");

        var token = _tokenService.GenerateToken(user);

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        Assert.That(jwt.ValidTo, Is.GreaterThan(DateTime.UtcNow));
    }
}