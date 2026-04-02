using AutoMapper;
using KanbanBoard.Application.DTOs.Requests;
using KanbanBoard.Application.DTOs.Requests.Board;
using KanbanBoard.Application.DTOs.Requests.Pagination;
using KanbanBoard.Application.DTOs.Responses;
using KanbanBoard.Application.Mapping;
using KanbanBoard.Application.Services;
using KanbanBoard.Domain.Entities;
using KanbanBoard.Domain.Interfaces.Repository;
using Microsoft.Extensions.Logging;
using Moq;

namespace KanbanBoard.Test;

[TestFixture]
public class BoardServiceTests
{
    //mocking the dependencies of the BoardService, including the IBoardRepository, IBoardMemberRepository, ILogger and IMapper.
    private Mock<IBoardRepository> _boardRepositoryMock;
    private Mock<IBoardMemberRepository> _boardMemberRepositoryMock;
    private Mock<ILogger<BoardService>> _loggerMock;
    private IMapper _mapper;
    private BoardService _boardService;

    [SetUp]
    public void Setup()
    {
        _boardRepositoryMock = new Mock<IBoardRepository>();
        _boardMemberRepositoryMock = new Mock<IBoardMemberRepository>();
        _loggerMock = new Mock<ILogger<BoardService>>();

        var loggerFactory = LoggerFactory.Create(b => b.AddConsole());
        var mapperConfig = new MapperConfiguration(cfg =>
            cfg.AddProfile<KanbanMapping>(), loggerFactory);
        _mapper = mapperConfig.CreateMapper();

        _boardService = new BoardService(
            _boardRepositoryMock.Object,
            _boardMemberRepositoryMock.Object,
            _mapper,
            _loggerMock.Object);
    }

    // Tests that CreateBoard returns a BoardResponseDto with correct details
    [Test]
    public async Task CreateBoardReturnsBoardResponseDto()
    {
        // Arrange
        var dto = new CreateBoardDto("Sprint 1");
        var userId = Guid.NewGuid().ToString();

        // Capture the Board object passed to the repository and return it as is
        _boardRepositoryMock
            .Setup(repo => repo.Add(It.IsAny<Board>()))
            .ReturnsAsync((Board b) => b);

        // Act
        var result = await _boardService.CreateBoard(dto, userId);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.TypeOf<BoardResponseDto>());
        Assert.That(result.Name, Is.EqualTo("Sprint 1"));
        Assert.That(result.CreatedBy, Is.EqualTo(userId));
    }

    // Tests that CreateBoard adds the creator as an Admin member
    [Test]
    public async Task CreateBoardAddsMemberWithAdminRole()
    {
        var dto = new CreateBoardDto("Sprint 1");
        var userId = Guid.NewGuid().ToString();

        Board capturedBoard = null;

        _boardRepositoryMock
            .Setup(repo => repo.Add(It.IsAny<Board>()))
            .Callback<Board>(b => capturedBoard = b)
            .ReturnsAsync((Board b) => b);

        await _boardService.CreateBoard(dto, userId);

        Assert.That(capturedBoard, Is.Not.Null);
        Assert.That(capturedBoard.Members.Any(m => m.UserId == userId && m.Role == "Admin"), Is.True);
    }

    // Tests that GetBoardById throws KeyNotFoundException when board does not exist
    [Test]
    public async Task GetBoardByIdThrowsKeyNotFoundWhenBoardDoesNotExist()
    {
        var boardId = Guid.NewGuid();
        var userId = Guid.NewGuid().ToString();

        _boardRepositoryMock
            .Setup(repo => repo.GetById(boardId))
            .ReturnsAsync((Board?)null);

        var ex = Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _boardService.GetBoardById(boardId, userId));

        Assert.That(ex.Message, Does.Contain(boardId.ToString()));
    }

    // Tests that GetBoardById throws UnauthorizedAccessException when user is not a member
    [Test]
    public async Task GetBoardByIdThrowsUnauthorizedWhenUserIsNotMember()
    {
        var boardId = Guid.NewGuid();
        var userId = Guid.NewGuid().ToString();

        _boardRepositoryMock
            .Setup(repo => repo.GetById(boardId))
            .ReturnsAsync(new Board { Id = boardId, Name = "Sprint 1" });

        _boardMemberRepositoryMock
            .Setup(repo => repo.GetByBoardAndUser(boardId, userId))
            .ReturnsAsync((BoardMember?)null);

        var ex = Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _boardService.GetBoardById(boardId, userId));

        Assert.That(ex.Message, Does.Contain("access"));
    }

    // Tests that GetBoardById returns BoardResponseDto when user is a member
    [Test]
    public async Task GetBoardByIdReturnsBoardResponseDtoWhenUserIsMember()
    {
        var boardId = Guid.NewGuid();
        var userId = Guid.NewGuid().ToString();

        _boardRepositoryMock
            .Setup(repo => repo.GetById(boardId))
            .ReturnsAsync(new Board { Id = boardId, Name = "Sprint 1" });

        _boardMemberRepositoryMock
            .Setup(repo => repo.GetByBoardAndUser(boardId, userId))
            .ReturnsAsync(new BoardMember { BoardId = boardId, UserId = userId, Role = "Member" });

        var result = await _boardService.GetBoardById(boardId, userId);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Name, Is.EqualTo("Sprint 1"));
    }

    // Tests that GetBoardsByUserId returns empty paged result when user has no boards
    [Test]
    public async Task GetBoardsByUserIdReturnsEmptyPagedResultWhenNoBoardsExist()
    {
        var userId = Guid.NewGuid().ToString();
        var pagination = new PaginationParams { Page = 1, PageSize = 10 };

        _boardRepositoryMock
            .Setup(repo => repo.GetByUserId(userId, pagination.Page, pagination.PageSize))
            .ReturnsAsync((new List<Board>(), 0));

        var result = await _boardService.GetBoardsByUserId(userId, pagination);

        Assert.That(result.Items, Is.Empty);
        Assert.That(result.TotalCount, Is.EqualTo(0));
    }

    // Tests that GetBoardsByUserId returns paged result with correct boards
    [Test]
    public async Task GetBoardsByUserIdReturnsPagedResult()
    {
        var userId = Guid.NewGuid().ToString();
        var pagination = new PaginationParams { Page = 1, PageSize = 10 };

        var boards = new List<Board>
        {
            new Board { Id = Guid.NewGuid(), Name = "Sprint 1", CreatedBy = userId },
            new Board { Id = Guid.NewGuid(), Name = "Sprint 2", CreatedBy = userId }
        };

        _boardRepositoryMock
            .Setup(repo => repo.GetByUserId(userId, pagination.Page, pagination.PageSize))
            .ReturnsAsync((boards, boards.Count));

        var result = await _boardService.GetBoardsByUserId(userId, pagination);

        Assert.That(result.Items.Count(), Is.EqualTo(2));
        Assert.That(result.TotalCount, Is.EqualTo(2));
        Assert.That(result.Items.Any(b => b.Name == "Sprint 1"), Is.True);
        Assert.That(result.Items.Any(b => b.Name == "Sprint 2"), Is.True);
    }

    // Tests that UpdateBoard throws KeyNotFoundException when board does not exist
    [Test]
    public async Task UpdateBoardThrowsKeyNotFoundWhenBoardDoesNotExist()
    {
        var boardId = Guid.NewGuid();
        var userId = Guid.NewGuid().ToString();

        _boardRepositoryMock
            .Setup(repo => repo.GetById(boardId))
            .ReturnsAsync((Board?)null);

        var ex = Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _boardService.UpdateBoard(boardId, new UpdateBoardDto("New Name"), userId));

        Assert.That(ex.Message, Does.Contain(boardId.ToString()));
    }

    // Tests that UpdateBoard throws UnauthorizedAccessException when user is not Admin
    [Test]
    public async Task UpdateBoardThrowsUnauthorizedWhenUserIsNotAdmin()
    {
        var boardId = Guid.NewGuid();
        var userId = Guid.NewGuid().ToString();

        _boardRepositoryMock
            .Setup(repo => repo.GetById(boardId))
            .ReturnsAsync(new Board { Id = boardId, Name = "Sprint 1" });

        _boardMemberRepositoryMock
            .Setup(repo => repo.GetByBoardAndUser(boardId, userId))
            .ReturnsAsync(new BoardMember { BoardId = boardId, UserId = userId, Role = "Member" });

        var ex = Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _boardService.UpdateBoard(boardId, new UpdateBoardDto("New Name"), userId));

        Assert.That(ex.Message, Does.Contain("Admin"));
    }

    // Tests that UpdateBoard returns updated BoardResponseDto when user is Admin
    [Test]
    public async Task UpdateBoardReturnsUpdatedBoardWhenUserIsAdmin()
    {
        var boardId = Guid.NewGuid();
        var userId = Guid.NewGuid().ToString();

        var existingBoard = new Board { Id = boardId, Name = "Sprint 1" };

        _boardRepositoryMock
            .Setup(repo => repo.GetById(boardId))
            .ReturnsAsync(existingBoard);

        _boardMemberRepositoryMock
            .Setup(repo => repo.GetByBoardAndUser(boardId, userId))
            .ReturnsAsync(new BoardMember { BoardId = boardId, UserId = userId, Role = "Admin" });

        _boardRepositoryMock
            .Setup(repo => repo.Update(It.IsAny<Board>()))
            .ReturnsAsync((Board b) => b);

        var result = await _boardService.UpdateBoard(boardId, new UpdateBoardDto("Sprint 2"), userId);

        Assert.That(result.Name, Is.EqualTo("Sprint 2"));
    }

    // Tests that DeleteBoard throws KeyNotFoundException when board does not exist
    [Test]
    public async Task DeleteBoardThrowsKeyNotFoundWhenBoardDoesNotExist()
    {
        var boardId = Guid.NewGuid();
        var userId = Guid.NewGuid().ToString();

        _boardRepositoryMock
            .Setup(repo => repo.GetById(boardId))
            .ReturnsAsync((Board?)null);

        var ex = Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _boardService.DeleteBoard(boardId, userId));

        Assert.That(ex.Message, Does.Contain(boardId.ToString()));
    }

    // Tests that DeleteBoard throws UnauthorizedAccessException when user is not Admin
    [Test]
    public async Task DeleteBoardThrowsUnauthorizedWhenUserIsNotAdmin()
    {
        var boardId = Guid.NewGuid();
        var userId = Guid.NewGuid().ToString();

        _boardRepositoryMock
            .Setup(repo => repo.GetById(boardId))
            .ReturnsAsync(new Board { Id = boardId, Name = "Sprint 1" });

        _boardMemberRepositoryMock
            .Setup(repo => repo.GetByBoardAndUser(boardId, userId))
            .ReturnsAsync(new BoardMember { BoardId = boardId, UserId = userId, Role = "Member" });

        var ex = Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _boardService.DeleteBoard(boardId, userId));

        Assert.That(ex.Message, Does.Contain("Admin"));
    }

    // Tests that DeleteBoard completes successfully when user is Admin
    [Test]
    public async Task DeleteBoardCompletesSuccessfullyWhenUserIsAdmin()
    {
        var boardId = Guid.NewGuid();
        var userId = Guid.NewGuid().ToString();

        _boardRepositoryMock
            .Setup(repo => repo.GetById(boardId))
            .ReturnsAsync(new Board { Id = boardId, Name = "Sprint 1" });

        _boardMemberRepositoryMock
            .Setup(repo => repo.GetByBoardAndUser(boardId, userId))
            .ReturnsAsync(new BoardMember { BoardId = boardId, UserId = userId, Role = "Admin" });

        _boardRepositoryMock
            .Setup(repo => repo.Delete(boardId))
            .ReturnsAsync(true);

        Assert.DoesNotThrowAsync(() => _boardService.DeleteBoard(boardId, userId));

        _boardRepositoryMock.Verify(repo => repo.Delete(boardId), Times.Once);
    }
}