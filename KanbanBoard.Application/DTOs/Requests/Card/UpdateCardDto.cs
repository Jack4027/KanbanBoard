namespace KanbanBoard.Application.DTOs.Requests.Card
{
    //Represents the user request data to update a card
    public record UpdateCardDto(string Title, string? Description);
}
