namespace KanbanBoard.Application.DTOs.Responses
{
    public record BoardMemberResponseDto
    {    
       public string UserId { get; init; }  = string.Empty;
        public string Role { get; init; } = string.Empty;
    
     }
}
