using AutoMapper;
using KanbanBoard.Application.DTOs.Auth;
using KanbanBoard.Application.DTOs.Auth.Requests;
using KanbanBoard.Application.Interfaces;
using KanbanBoard.Application.Interfaces.Identity;
using KanbanBoard.Application.Mapping;
using KanbanBoard.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;

namespace KanbanBoard.Test;

[TestFixture]
public class AuthServiceTests
{
    private Mock<UserManager<AppUserIdentity>> _userManagerMock;
    private Mock<ITokenService> _tokenServiceMock;
    private Mock<ILogger<AuthService>> _loggerMock;
    private IMapper _mapper;
    private AuthService _authService;

    [SetUp]
    public void Setup()
    {
        _userManagerMock = new Mock<UserManager<AppUserIdentity>>(
            Mock.Of<IUserStore<AppUserIdentity>>(),
            null, null, null, null, null, null, null, null);

        _tokenServiceMock = new Mock<ITokenService>();
        _loggerMock = new Mock<ILogger<AuthService>>();

        var loggerFactory = LoggerFactory.Create(b => b.AddConsole());
        var mapperConfig = new MapperConfiguration(cfg =>
            cfg.AddProfile<IdentityMappingProfile>(), loggerFactory);
        _mapper = mapperConfig.CreateMapper();

        _authService = new AuthService(
            _userManagerMock.Object,
            _tokenServiceMock.Object,
            _mapper,
            _loggerMock.Object);
    }

    // Tests that Register throws InvalidOperationException when email already exists
    [Test]
    public async Task RegisterThrowsInvalidOperationWhenEmailAlreadyExists()
    {
        var dto = new RegisterDto("John", "Doe", "john@kanban.com", "Password1!");

        _userManagerMock
            .Setup(m => m.FindByEmailAsync(dto.Email))
            .ReturnsAsync(new AppUserIdentity { Email = dto.Email });

        var ex = Assert.ThrowsAsync<InvalidOperationException>(() =>
            _authService.Register(dto));

        Assert.That(ex.Message, Does.Contain(dto.Email));
    }

    // Tests that Register throws InvalidOperationException when Identity returns errors
    [Test]
    public async Task RegisterThrowsInvalidOperationWhenIdentityFails()
    {
        var dto = new RegisterDto("John", "Doe", "john@kanban.com", "weak");

        _userManagerMock
            .Setup(m => m.FindByEmailAsync(dto.Email))
            .ReturnsAsync((AppUserIdentity?)null);

        _userManagerMock
            .Setup(m => m.CreateAsync(It.IsAny<AppUserIdentity>(), dto.Password))
            .ReturnsAsync(IdentityResult.Failed(
                new IdentityError { Description = "Password too weak." }));

        var ex = Assert.ThrowsAsync<InvalidOperationException>(() =>
            _authService.Register(dto));

        Assert.That(ex.Message, Does.Contain("Password too weak."));
    }

    // Tests that Register returns AuthResponseDto on successful registration
    [Test]
    public async Task RegisterReturnsAuthResponseDtoOnSuccess()
    {
        var dto = new RegisterDto("John", "Doe", "john@kanban.com", "Password1!");

        _userManagerMock
            .Setup(m => m.FindByEmailAsync(dto.Email))
            .ReturnsAsync((AppUserIdentity?)null);

        _userManagerMock
            .Setup(m => m.CreateAsync(It.IsAny<AppUserIdentity>(), dto.Password))
            .ReturnsAsync(IdentityResult.Success);

        _tokenServiceMock
            .Setup(t => t.GenerateToken(It.IsAny<AuthUserDto>()))
            .Returns("mocked-jwt-token");

        var result = await _authService.Register(dto);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Token, Is.EqualTo("mocked-jwt-token"));
        Assert.That(result.Email, Is.EqualTo(dto.Email));
        Assert.That(result.FullName, Is.EqualTo("John Doe"));
    }

    // Tests that Login throws KeyNotFoundException when email does not exist
    [Test]
    public async Task LoginThrowsKeyNotFoundWhenEmailDoesNotExist()
    {
        var dto = new LoginDto("john@kanban.com", "Password1!");

        _userManagerMock
            .Setup(m => m.FindByEmailAsync(dto.Email))
            .ReturnsAsync((AppUserIdentity?)null);

        var ex = Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _authService.Login(dto));

        Assert.That(ex.Message, Does.Contain("Invalid email or password"));
    }

    // Tests that Login throws KeyNotFoundException when password is incorrect
    [Test]
    public async Task LoginThrowsKeyNotFoundWhenPasswordIsIncorrect()
    {
        var dto = new LoginDto("john@kanban.com", "WrongPassword1!");

        _userManagerMock
            .Setup(m => m.FindByEmailAsync(dto.Email))
            .ReturnsAsync(new AppUserIdentity { Email = dto.Email });

        _userManagerMock
            .Setup(m => m.CheckPasswordAsync(It.IsAny<AppUserIdentity>(), dto.Password))
            .ReturnsAsync(false);

        var ex = Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _authService.Login(dto));

        Assert.That(ex.Message, Does.Contain("Invalid email or password"));
    }

    // Tests that Login returns AuthResponseDto on successful login
    [Test]
    public async Task LoginReturnsAuthResponseDtoOnSuccess()
    {
        var dto = new LoginDto("john@kanban.com", "Password1!");

        var user = new AppUserIdentity
        {
            Email = dto.Email,
            FirstName = "John",
            LastName = "Doe"
        };

        _userManagerMock
            .Setup(m => m.FindByEmailAsync(dto.Email))
            .ReturnsAsync(user);

        _userManagerMock
            .Setup(m => m.CheckPasswordAsync(user, dto.Password))
            .ReturnsAsync(true);

        _tokenServiceMock
            .Setup(t => t.GenerateToken(It.IsAny<AuthUserDto>()))
            .Returns("mocked-jwt-token");

        var result = await _authService.Login(dto);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Token, Is.EqualTo("mocked-jwt-token"));
        Assert.That(result.Email, Is.EqualTo(dto.Email));
        Assert.That(result.FullName, Is.EqualTo("John Doe"));
    }
}