namespace KanbanBoard.Application.DTOs.Auth.Requests
{
    //To be passed into the IToken Service in order to generate the JWT token 
    public record AuthUserDto(
    string Id,
    string Email,
    string FirstName,
    string LastName);
}
