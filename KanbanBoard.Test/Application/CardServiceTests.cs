using AutoMapper;
using KanbanBoard.Application.DTOs.Requests;
using KanbanBoard.Application.DTOs.Requests.Card;
using KanbanBoard.Application.DTOs.Responses;
using KanbanBoard.Application.Interfaces.Hubs;
using KanbanBoard.Application.Mapping;
using KanbanBoard.Application.Services;
using KanbanBoard.Domain.Entities;
using KanbanBoard.Domain.Interfaces;
using KanbanBoard.Domain.Interfaces.Repository;
using Microsoft.Extensions.Logging;
using Moq;

namespace KanbanBoard.Test;

[TestFixture]
public class CardServiceTests
{
    private Mock<ICardRepository> _cardRepositoryMock;
    private Mock<IColumnRepository> _columnRepositoryMock;
    private Mock<IBoardMemberRepository> _boardMemberRepositoryMock;
    private Mock<IKanbanNotificationService> _notificationServiceMock;
    private Mock<ILogger<CardService>> _loggerMock;
    private IMapper _mapper;
    private CardService _cardService;

    [SetUp]
    public void Setup()
    {
        _cardRepositoryMock = new Mock<ICardRepository>();
        _columnRepositoryMock = new Mock<IColumnRepository>();
        _boardMemberRepositoryMock = new Mock<IBoardMemberRepository>();
        _notificationServiceMock = new Mock<IKanbanNotificationService>();
        _loggerMock = new Mock<ILogger<CardService>>();

        var loggerFactory = LoggerFactory.Create(b => b.AddConsole());
        var mapperConfig = new MapperConfiguration(cfg =>
            cfg.AddProfile<KanbanMapping>(), loggerFactory);
        _mapper = mapperConfig.CreateMapper();

        _cardService = new CardService(
            _cardRepositoryMock.Object,
            _columnRepositoryMock.Object,
            _boardMemberRepositoryMock.Object,
            _notificationServiceMock.Object,
            _mapper,
            _loggerMock.Object);
    }

    // Tests that CreateCard throws KeyNotFoundException when column does not exist
    [Test]
    public async Task CreateCardThrowsKeyNotFoundWhenColumnDoesNotExist()
    {
        var columnId = Guid.NewGuid();
        var userId = Guid.NewGuid().ToString();

        _columnRepositoryMock
            .Setup(repo => repo.GetById(columnId))
            .ReturnsAsync((Column?)null);

        var ex = Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _cardService.CreateCard(columnId, new CreateCardDto("Task 1", null), userId));

        Assert.That(ex.Message, Does.Contain(columnId.ToString()));
    }

    // Tests that CreateCard throws UnauthorizedAccessException when user is not a board member
    [Test]
    public async Task CreateCardThrowsUnauthorizedWhenUserIsNotMember()
    {
        var columnId = Guid.NewGuid();
        var boardId = Guid.NewGuid();
        var userId = Guid.NewGuid().ToString();

        _columnRepositoryMock
            .Setup(repo => repo.GetById(columnId))
            .ReturnsAsync(new Column { Id = columnId, BoardId = boardId, Name = "To Do" });

        _boardMemberRepositoryMock
            .Setup(repo => repo.GetByBoardAndUser(boardId, userId))
            .ReturnsAsync((BoardMember?)null);

        var ex = Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _cardService.CreateCard(columnId, new CreateCardDto("Task 1", null), userId));

        Assert.That(ex.Message, Does.Contain("access"));
    }

    // Tests that CreateCard returns correct response and broadcasts notification
    [Test]
    public async Task CreateCardReturnsResponseAndBroadcastsNotification()
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

        _cardRepositoryMock
            .Setup(repo => repo.Add(It.IsAny<Card>()))
            .ReturnsAsync((Card c) => c);

        _notificationServiceMock
            .Setup(n => n.CardCreated(It.IsAny<string>(), It.IsAny<CardResponseDto>()))
            .Returns(Task.CompletedTask);

        var result = await _cardService.CreateCard(columnId, new CreateCardDto("Task 1", "Description"), userId);

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.TypeOf<CardResponseDto>());
        Assert.That(result.Title, Is.EqualTo("Task 1"));
        Assert.That(result.Description, Is.EqualTo("Description"));
        Assert.That(result.ColumnId, Is.EqualTo(columnId));

        _notificationServiceMock.Verify(
            n => n.CardCreated(boardId.ToString(), It.IsAny<CardResponseDto>()),
            Times.Once);
    }

    // Tests that UpdateCard throws KeyNotFoundException when card does not exist
    [Test]
    public async Task UpdateCardThrowsKeyNotFoundWhenCardDoesNotExist()
    {
        var cardId = Guid.NewGuid();
        var userId = Guid.NewGuid().ToString();

        _cardRepositoryMock
            .Setup(repo => repo.GetById(cardId))
            .ReturnsAsync((Card?)null);

        var ex = Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _cardService.UpdateCard(cardId, new UpdateCardDto("Updated", null), userId));

        Assert.That(ex.Message, Does.Contain(cardId.ToString()));
    }

    // Tests that UpdateCard throws UnauthorizedAccessException when user is not a board member
    [Test]
    public async Task UpdateCardThrowsUnauthorizedWhenUserIsNotMember()
    {
        var cardId = Guid.NewGuid();
        var columnId = Guid.NewGuid();
        var boardId = Guid.NewGuid();
        var userId = Guid.NewGuid().ToString();

        _cardRepositoryMock
            .Setup(repo => repo.GetById(cardId))
            .ReturnsAsync(new Card { Id = cardId, ColumnId = columnId, Title = "Task 1" });

        _columnRepositoryMock
            .Setup(repo => repo.GetById(columnId))
            .ReturnsAsync(new Column { Id = columnId, BoardId = boardId, Name = "To Do" });

        _boardMemberRepositoryMock
            .Setup(repo => repo.GetByBoardAndUser(boardId, userId))
            .ReturnsAsync((BoardMember?)null);

        var ex = Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _cardService.UpdateCard(cardId, new UpdateCardDto("Updated", null), userId));

        Assert.That(ex.Message, Does.Contain("access"));
    }

    // Tests that UpdateCard returns updated CardResponseDto
    [Test]
    public async Task UpdateCardReturnsUpdatedCardResponseDto()
    {
        var cardId = Guid.NewGuid();
        var columnId = Guid.NewGuid();
        var boardId = Guid.NewGuid();
        var userId = Guid.NewGuid().ToString();

        _cardRepositoryMock
            .Setup(repo => repo.GetById(cardId))
            .ReturnsAsync(new Card { Id = cardId, ColumnId = columnId, Title = "Task 1" });

        _columnRepositoryMock
            .Setup(repo => repo.GetById(columnId))
            .ReturnsAsync(new Column { Id = columnId, BoardId = boardId, Name = "To Do" });

        _boardMemberRepositoryMock
            .Setup(repo => repo.GetByBoardAndUser(boardId, userId))
            .ReturnsAsync(new BoardMember { BoardId = boardId, UserId = userId, Role = "Member" });

        _cardRepositoryMock
            .Setup(repo => repo.Update(It.IsAny<Card>()))
            .ReturnsAsync((Card c) => c);

        var result = await _cardService.UpdateCard(cardId, new UpdateCardDto("Updated Task", "New description"), userId);

        Assert.That(result.Title, Is.EqualTo("Updated Task"));
        Assert.That(result.Description, Is.EqualTo("New description"));
    }

    // Tests that MoveCard throws KeyNotFoundException when card does not exist
    [Test]
    public async Task MoveCardThrowsKeyNotFoundWhenCardDoesNotExist()
    {
        var cardId = Guid.NewGuid();
        var userId = Guid.NewGuid().ToString();

        _cardRepositoryMock
            .Setup(repo => repo.GetById(cardId))
            .ReturnsAsync((Card?)null);

        var ex = Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _cardService.MoveCard(cardId, new MoveCardDto(Guid.NewGuid()), userId));

        Assert.That(ex.Message, Does.Contain(cardId.ToString()));
    }

    // Tests that MoveCard throws InvalidOperationException when target column is same as source
    [Test]
    public async Task MoveCardThrowsInvalidOperationWhenTargetColumnIsSameAsSource()
    {
        var cardId = Guid.NewGuid();
        var columnId = Guid.NewGuid();
        var userId = Guid.NewGuid().ToString();

        _cardRepositoryMock
            .Setup(repo => repo.GetById(cardId))
            .ReturnsAsync(new Card { Id = cardId, ColumnId = columnId, Title = "Task 1" });

        var ex = Assert.ThrowsAsync<InvalidOperationException>(() =>
            _cardService.MoveCard(cardId, new MoveCardDto(columnId), userId));

        Assert.That(ex.Message, Does.Contain("already in the target column"));
    }

    // Tests that MoveCard throws InvalidOperationException when target column is on a different board
    [Test]
    public async Task MoveCardThrowsInvalidOperationWhenTargetColumnIsOnDifferentBoard()
    {
        var cardId = Guid.NewGuid();
        var sourceColumnId = Guid.NewGuid();
        var targetColumnId = Guid.NewGuid();
        var userId = Guid.NewGuid().ToString();

        _cardRepositoryMock
            .Setup(repo => repo.GetById(cardId))
            .ReturnsAsync(new Card { Id = cardId, ColumnId = sourceColumnId, Title = "Task 1" });

        _columnRepositoryMock
            .Setup(repo => repo.GetById(sourceColumnId))
            .ReturnsAsync(new Column { Id = sourceColumnId, BoardId = Guid.NewGuid(), Name = "To Do" });

        _columnRepositoryMock
            .Setup(repo => repo.GetById(targetColumnId))
            .ReturnsAsync(new Column { Id = targetColumnId, BoardId = Guid.NewGuid(), Name = "Done" });

        var ex = Assert.ThrowsAsync<InvalidOperationException>(() =>
            _cardService.MoveCard(cardId, new MoveCardDto(targetColumnId), userId));

        Assert.That(ex.Message, Does.Contain("different board"));
    }

    // Tests that MoveCard throws UnauthorizedAccessException when user is not a board member
    [Test]
    public async Task MoveCardThrowsUnauthorizedWhenUserIsNotMember()
    {
        var cardId = Guid.NewGuid();
        var sourceColumnId = Guid.NewGuid();
        var targetColumnId = Guid.NewGuid();
        var boardId = Guid.NewGuid();
        var userId = Guid.NewGuid().ToString();

        _cardRepositoryMock
            .Setup(repo => repo.GetById(cardId))
            .ReturnsAsync(new Card { Id = cardId, ColumnId = sourceColumnId, Title = "Task 1" });

        _columnRepositoryMock
            .Setup(repo => repo.GetById(sourceColumnId))
            .ReturnsAsync(new Column { Id = sourceColumnId, BoardId = boardId, Name = "To Do" });

        _columnRepositoryMock
            .Setup(repo => repo.GetById(targetColumnId))
            .ReturnsAsync(new Column { Id = targetColumnId, BoardId = boardId, Name = "Done" });

        _boardMemberRepositoryMock
            .Setup(repo => repo.GetByBoardAndUser(boardId, userId))
            .ReturnsAsync((BoardMember?)null);

        var ex = Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _cardService.MoveCard(cardId, new MoveCardDto(targetColumnId), userId));

        Assert.That(ex.Message, Does.Contain("access"));
    }

    // Tests that MoveCard returns correct response and broadcasts notification
    [Test]
    public async Task MoveCardReturnsResponseAndBroadcastsNotification()
    {
        var cardId = Guid.NewGuid();
        var sourceColumnId = Guid.NewGuid();
        var targetColumnId = Guid.NewGuid();
        var boardId = Guid.NewGuid();
        var userId = Guid.NewGuid().ToString();

        _cardRepositoryMock
            .Setup(repo => repo.GetById(cardId))
            .ReturnsAsync(new Card { Id = cardId, ColumnId = sourceColumnId, Title = "Task 1" });

        _columnRepositoryMock
            .Setup(repo => repo.GetById(sourceColumnId))
            .ReturnsAsync(new Column { Id = sourceColumnId, BoardId = boardId, Name = "To Do" });

        _columnRepositoryMock
            .Setup(repo => repo.GetById(targetColumnId))
            .ReturnsAsync(new Column { Id = targetColumnId, BoardId = boardId, Name = "Done" });

        _boardMemberRepositoryMock
            .Setup(repo => repo.GetByBoardAndUser(boardId, userId))
            .ReturnsAsync(new BoardMember { BoardId = boardId, UserId = userId, Role = "Member" });

        _cardRepositoryMock
            .Setup(repo => repo.MoveCard(cardId, targetColumnId))
            .ReturnsAsync(new Card { Id = cardId, ColumnId = targetColumnId, Title = "Task 1" });

        _notificationServiceMock
            .Setup(n => n.CardMoved(It.IsAny<string>(), It.IsAny<CardResponseDto>()))
            .Returns(Task.CompletedTask);

        var result = await _cardService.MoveCard(cardId, new MoveCardDto(targetColumnId), userId);

        Assert.That(result.ColumnId, Is.EqualTo(targetColumnId));

        _notificationServiceMock.Verify(
            n => n.CardMoved(boardId.ToString(), It.IsAny<CardResponseDto>()),
            Times.Once);
    }

    // Tests that DeleteCard throws KeyNotFoundException when card does not exist
    [Test]
    public async Task DeleteCardThrowsKeyNotFoundWhenCardDoesNotExist()
    {
        var cardId = Guid.NewGuid();
        var userId = Guid.NewGuid().ToString();

        _cardRepositoryMock
            .Setup(repo => repo.GetById(cardId))
            .ReturnsAsync((Card?)null);

        var ex = Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _cardService.DeleteCard(cardId, userId));

        Assert.That(ex.Message, Does.Contain(cardId.ToString()));
    }

    // Tests that DeleteCard throws UnauthorizedAccessException when user is not a board member
    [Test]
    public async Task DeleteCardThrowsUnauthorizedWhenUserIsNotMember()
    {
        var cardId = Guid.NewGuid();
        var columnId = Guid.NewGuid();
        var boardId = Guid.NewGuid();
        var userId = Guid.NewGuid().ToString();

        _cardRepositoryMock
            .Setup(repo => repo.GetById(cardId))
            .ReturnsAsync(new Card { Id = cardId, ColumnId = columnId, Title = "Task 1" });

        _columnRepositoryMock
            .Setup(repo => repo.GetById(columnId))
            .ReturnsAsync(new Column { Id = columnId, BoardId = boardId, Name = "To Do" });

        _boardMemberRepositoryMock
            .Setup(repo => repo.GetByBoardAndUser(boardId, userId))
            .ReturnsAsync((BoardMember?)null);

        var ex = Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _cardService.DeleteCard(cardId, userId));

        Assert.That(ex.Message, Does.Contain("access"));
    }

    // Tests that DeleteCard completes successfully and broadcasts notification
    [Test]
    public async Task DeleteCardCompletesSuccessfullyAndBroadcastsNotification()
    {
        var cardId = Guid.NewGuid();
        var columnId = Guid.NewGuid();
        var boardId = Guid.NewGuid();
        var userId = Guid.NewGuid().ToString();

        _cardRepositoryMock
            .Setup(repo => repo.GetById(cardId))
            .ReturnsAsync(new Card { Id = cardId, ColumnId = columnId, Title = "Task 1" });

        _columnRepositoryMock
            .Setup(repo => repo.GetById(columnId))
            .ReturnsAsync(new Column { Id = columnId, BoardId = boardId, Name = "To Do" });

        _boardMemberRepositoryMock
            .Setup(repo => repo.GetByBoardAndUser(boardId, userId))
            .ReturnsAsync(new BoardMember { BoardId = boardId, UserId = userId, Role = "Member" });

        _cardRepositoryMock
            .Setup(repo => repo.Delete(cardId))
            .ReturnsAsync(true);

        _notificationServiceMock
            .Setup(n => n.CardDeleted(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
            .Returns(Task.CompletedTask);

        Assert.DoesNotThrowAsync(() => _cardService.DeleteCard(cardId, userId));

        _cardRepositoryMock.Verify(repo => repo.Delete(cardId), Times.Once);

        _notificationServiceMock.Verify(
            n => n.CardDeleted(boardId.ToString(), cardId, columnId),
            Times.Once);
    }
}