namespace KanbanBoard.Application.DTOs.Requests.Card
{
    //Represents the user request data to create a card
    public record CreateCardDto(string Title, string? Description);
}
