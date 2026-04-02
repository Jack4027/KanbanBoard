namespace KanbanBoard.Application.DTOs.Responses
{
    //The data sent back to angular to represent a board member, including the user id and the role of the user in the board
    public record BoardMemberResponseDto
    {    
       public string UserId { get; init; }  = string.Empty;
        public string Role { get; init; } = string.Empty;
    
     }
}
