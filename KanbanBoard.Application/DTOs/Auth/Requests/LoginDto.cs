namespace KanbanBoard.Application.DTOs.Auth.Requests
{
    //Login details passed buy the user
    public record LoginDto(string Email, string Password);
}
