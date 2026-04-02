using AutoMapper;
using KanbanBoard.Application.DTOs.Requests.Column;
using KanbanBoard.Application.DTOs.Responses;
using KanbanBoard.Application.Mapping;
using KanbanBoard.Application.Services;
using KanbanBoard.Domain.Entities;
using KanbanBoard.Domain.Interfaces.Repository;
using Microsoft.Extensions.Logging;
using Moq;

namespace KanbanBoard.Test;

[TestFixture]
public class ColumnServiceTests
{
    private Mock<IColumnRepository> _columnRepositoryMock;
    private Mock<IBoardRepository> _boardRepositoryMock;
    private Mock<IBoardMemberRepository> _boardMemberRepositoryMock;
    private Mock<ILogger<ColumnService>> _loggerMock;
    private IMapper _mapper;
    private ColumnService _columnService;

    [SetUp]
    public void Setup()
    {
        _columnRepositoryMock = new Mock<IColumnRepository>();
        _boardRepositoryMock = new Mock<IBoardRepository>();
        _boardMemberRepositoryMock = new Mock<IBoardMemberRepository>();
        _loggerMock = new Mock<ILogger<ColumnService>>();

        var loggerFactory = LoggerFactory.Create(b => b.AddConsole());
        var mapperConfig = new MapperConfiguration(cfg =>
            cfg.AddProfile<KanbanMapping>(), loggerFactory);
        _mapper = mapperConfig.CreateMapper();

        _columnService = new ColumnService(
            _columnRepositoryMock.Object,
            _boardRepositoryMock.Object,
            _boardMemberRepositoryMock.Object,
            _mapper,
            _loggerMock.Object);
    }

    // Tests that CreateColumn throws KeyNotFoundException when board does not exist
    [Test]
    public async Task CreateColumnThrowsKeyNotFoundWhenBoardDoesNotExist()
    {
        var boardId = Guid.NewGuid();
        var userId = Guid.NewGuid().ToString();

        _boardRepositoryMock
            .Setup(repo => repo.GetById(boardId))
            .ReturnsAsync((Board?)null);

        var ex = Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _columnService.CreateColumn(boardId, new CreateColumnDto("To Do"), userId));

        Assert.That(ex.Message, Does.Contain(boardId.ToString()));
    }

    // Tests that CreateColumn throws UnauthorizedAccessException when user is not Admin
    [Test]
    public async Task CreateColumnThrowsUnauthorizedWhenUserIsNotAdmin()
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
            _columnService.CreateColumn(boardId, new CreateColumnDto("To Do"), userId));

        Assert.That(ex.Message, Does.Contain("Admin"));
    }

    // Tests that CreateColumn throws InvalidOperationException when column name already exists on board
    [Test]
    public async Task CreateColumnThrowsInvalidOperationWhenColumnNameAlreadyExists()
    {
        var boardId = Guid.NewGuid();
        var userId = Guid.NewGuid().ToString();

        _boardRepositoryMock
            .Setup(repo => repo.GetById(boardId))
            .ReturnsAsync(new Board { Id = boardId, Name = "Sprint 1" });

        _boardMemberRepositoryMock
            .Setup(repo => repo.GetByBoardAndUser(boardId, userId))
            .ReturnsAsync(new BoardMember { BoardId = boardId, UserId = userId, Role = "Admin" });

        _columnRepositoryMock
            .Setup(repo => repo.ExistsWithNameOnBoard(boardId, "To Do", null))
            .ReturnsAsync(true);

        var ex = Assert.ThrowsAsync<InvalidOperationException>(() =>
            _columnService.CreateColumn(boardId, new CreateColumnDto("To Do"), userId));

        Assert.That(ex.Message, Does.Contain("To Do"));
    }

    // Tests that CreateColumn returns ColumnResponseDto with correct details
    [Test]
    public async Task CreateColumnReturnsColumnResponseDto()
    {
        var boardId = Guid.NewGuid();
        var userId = Guid.NewGuid().ToString();

        _boardRepositoryMock
            .Setup(repo => repo.GetById(boardId))
            .ReturnsAsync(new Board { Id = boardId, Name = "Sprint 1", Columns = [] });

        _boardMemberRepositoryMock
            .Setup(repo => repo.GetByBoardAndUser(boardId, userId))
            .ReturnsAsync(new BoardMember { BoardId = boardId, UserId = userId, Role = "Admin" });

        _columnRepositoryMock
            .Setup(repo => repo.ExistsWithNameOnBoard(boardId, "To Do", null))
            .ReturnsAsync(false);

        _columnRepositoryMock
            .Setup(repo => repo.Add(It.IsAny<Column>()))
            .ReturnsAsync((Column c) => c);

        var result = await _columnService.CreateColumn(boardId, new CreateColumnDto("To Do"), userId);

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.TypeOf<ColumnResponseDto>());
        Assert.That(result.Name, Is.EqualTo("To Do"));
        Assert.That(result.BoardId, Is.EqualTo(boardId));
    }

    // Tests that CreateColumn sets position based on existing column count
    [Test]
    public async Task CreateColumnSetsPositionBasedOnExistingColumnCount()
    {
        var boardId = Guid.NewGuid();
        var userId = Guid.NewGuid().ToString();

        var existingColumns = new List<Column>
        {
            new Column { Id = Guid.NewGuid(), BoardId = boardId, Name = "To Do", Position = 0 },
            new Column { Id = Guid.NewGuid(), BoardId = boardId, Name = "In Progress", Position = 1 }
        };

        _boardRepositoryMock
            .Setup(repo => repo.GetById(boardId))
            .ReturnsAsync(new Board { Id = boardId, Name = "Sprint 1", Columns = existingColumns });

        _boardMemberRepositoryMock
            .Setup(repo => repo.GetByBoardAndUser(boardId, userId))
            .ReturnsAsync(new BoardMember { BoardId = boardId, UserId = userId, Role = "Admin" });

        _columnRepositoryMock
            .Setup(repo => repo.ExistsWithNameOnBoard(boardId, "Done", null))
            .ReturnsAsync(false);

        Column capturedColumn = null;

        _columnRepositoryMock
            .Setup(repo => repo.Add(It.IsAny<Column>()))
            .Callback<Column>(c => capturedColumn = c)
            .ReturnsAsync((Column c) => c);

        await _columnService.CreateColumn(boardId, new CreateColumnDto("Done"), userId);

        Assert.That(capturedColumn!.Position, Is.EqualTo(2));
    }

    // Tests that UpdateColumn throws KeyNotFoundException when column does not exist
    [Test]
    public async Task UpdateColumnThrowsKeyNotFoundWhenColumnDoesNotExist()
    {
        var columnId = Guid.NewGuid();
        var userId = Guid.NewGuid().ToString();

        _columnRepositoryMock
            .Setup(repo => repo.GetById(columnId))
            .ReturnsAsync((Column?)null);

        var ex = Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _columnService.UpdateColumn(columnId, new UpdateColumnDto("New Name"), userId));

        Assert.That(ex.Message, Does.Contain(columnId.ToString()));
    }

    // Tests that UpdateColumn throws UnauthorizedAccessException when user is not Admin
    [Test]
    public async Task UpdateColumnThrowsUnauthorizedWhenUserIsNotAdmin()
    {
        var columnId = Guid.NewGuid();
        var boardId = Guid.NewGuid();
        var userId = Guid.NewGuid().ToString();

        _columnRepositoryMock
            .Setup(repo => repo.GetById(columnId))
            .ReturnsAsync(new Column { Id = columnId, BoardId = boardId, Name = "To Do" });

        _boardMemberRepositoryMock
            .Setup(repo => repo.GetByBoardAndUser(boardId, userId))
            .ReturnsAsync(new BoardMember { BoardId = boardId, UserId = userId, Role = "Member" });

        var ex = Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _columnService.UpdateColumn(columnId, new UpdateColumnDto("New Name"), userId));

        Assert.That(ex.Message, Does.Contain("Admin"));
    }

    // Tests that UpdateColumn throws InvalidOperationException when new name already exists on board
    [Test]
    public async Task UpdateColumnThrowsInvalidOperationWhenNewNameAlreadyExists()
    {
        var columnId = Guid.NewGuid();
        var boardId = Guid.NewGuid();
        var userId = Guid.NewGuid().ToString();

        _columnRepositoryMock
            .Setup(repo => repo.GetById(columnId))
            .ReturnsAsync(new Column { Id = columnId, BoardId = boardId, Name = "To Do" });

        _boardMemberRepositoryMock
            .Setup(repo => repo.GetByBoardAndUser(boardId, userId))
            .ReturnsAsync(new BoardMember { BoardId = boardId, UserId = userId, Role = "Admin" });

        _boardRepositoryMock
            .Setup(repo => repo.GetById(boardId))
            .ReturnsAsync(new Board { Id = boardId, Name = "Test Board" });

        _columnRepositoryMock
            .Setup(repo => repo.ExistsWithNameOnBoard(boardId, "In Progress", columnId))
            .ReturnsAsync(true);

        var ex = Assert.ThrowsAsync<InvalidOperationException>(() =>
            _columnService.UpdateColumn(columnId, new UpdateColumnDto("In Progress"), userId));

        Assert.That(ex.Message, Does.Contain("In Progress"));
    }

    // Tests that UpdateColumn returns updated ColumnResponseDto when user is Admin
    [Test]
    public async Task UpdateColumnReturnsUpdatedColumnWhenUserIsAdmin()
    {
        var columnId = Guid.NewGuid();
        var boardId = Guid.NewGuid();
        var userId = Guid.NewGuid().ToString();

        var existingColumn = new Column { Id = columnId, BoardId = boardId, Name = "To Do" };

        _columnRepositoryMock
            .Setup(repo => repo.GetById(columnId))
            .ReturnsAsync(existingColumn);

        _boardMemberRepositoryMock
            .Setup(repo => repo.GetByBoardAndUser(boardId, userId))
            .ReturnsAsync(new BoardMember { BoardId = boardId, UserId = userId, Role = "Admin" });

        _boardRepositoryMock
            .Setup(repo => repo.GetById(boardId))
            .ReturnsAsync(new Board { Id = boardId, Name = "Test Board" });

        _columnRepositoryMock
            .Setup(repo => repo.ExistsWithNameOnBoard(boardId, "In Progress", columnId))
            .ReturnsAsync(false);

        _columnRepositoryMock
            .Setup(repo => repo.Update(It.IsAny<Column>()))
            .ReturnsAsync((Column c) => c);

        var result = await _columnService.UpdateColumn(columnId, new UpdateColumnDto("In Progress"), userId);

        Assert.That(result.Name, Is.EqualTo("In Progress"));
    }

    // Tests that DeleteColumn throws KeyNotFoundException when column does not exist
    [Test]
    public async Task DeleteColumnThrowsKeyNotFoundWhenColumnDoesNotExist()
    {
        var columnId = Guid.NewGuid();
        var userId = Guid.NewGuid().ToString();

        _columnRepositoryMock
            .Setup(repo => repo.GetById(columnId))
            .ReturnsAsync((Column?)null);

        var ex = Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _columnService.DeleteColumn(columnId, userId));

        Assert.That(ex.Message, Does.Contain(columnId.ToString()));
    }

    // Tests that DeleteColumn throws UnauthorizedAccessException when user is not Admin
    [Test]
    public async Task DeleteColumnThrowsUnauthorizedWhenUserIsNotAdmin()
    {
        var columnId = Guid.NewGuid();
        var boardId = Guid.NewGuid();
        var userId = Guid.NewGuid().ToString();

        _columnRepositoryMock
            .Setup(repo => repo.GetById(columnId))
            .ReturnsAsync(new Column { Id = columnId, BoardId = boardId, Name = "To Do" });

        _boardMemberRepositoryMock
            .Setup(repo => repo.GetByBoardAndUser(boardId, userId))
            .ReturnsAsync(new BoardMember { BoardId = boardId, UserId = userId, Role = "Member" });

        var ex = Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _columnService.DeleteColumn(columnId, userId));

        Assert.That(ex.Message, Does.Contain("Admin"));
    }

    // Tests that DeleteColumn completes successfully when user is Admin
    [Test]
    public async Task DeleteColumnCompletesSuccessfullyWhenUserIsAdmin()
    {
        var columnId = Guid.NewGuid();
        var boardId = Guid.NewGuid();
        var userId = Guid.NewGuid().ToString();

        _columnRepositoryMock
            .Setup(repo => repo.GetById(columnId))
            .ReturnsAsync(new Column { Id = columnId, BoardId = boardId, Name = "To Do" });

        _boardMemberRepositoryMock
            .Setup(repo => repo.GetByBoardAndUser(boardId, userId))
            .ReturnsAsync(new BoardMember { BoardId = boardId, UserId = userId, Role = "Admin" });

        _columnRepositoryMock
            .Setup(repo => repo.Delete(columnId))
            .ReturnsAsync(true);

        Assert.DoesNotThrowAsync(() => _columnService.DeleteColumn(columnId, userId));

        _columnRepositoryMock.Verify(repo => repo.Delete(columnId), Times.Once);
    }
}