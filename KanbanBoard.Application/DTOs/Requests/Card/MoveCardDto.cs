namespace KanbanBoard.Application.DTOs.Requests.Card
{
    //Represents the user request data to move a card
    public record MoveCardDto(Guid TargetColumnId);
}
