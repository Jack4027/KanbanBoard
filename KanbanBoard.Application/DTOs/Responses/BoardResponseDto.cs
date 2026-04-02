namespace KanbanBoard.Application.DTOs.Responses
{
    //Object returned back to the client to represent a board, it includes the board details such as id, name, creator and creation date, as well as the columns and members of the board
    public record BoardResponseDto
    {
        public Guid Id {get; init;}
        public string Name { get; init; } = string.Empty;    
        public string CreatedBy { get; init; } = string.Empty;
        public DateTime CreatedAt { get; init; }
        public List<ColumnResponseDto> Columns {get; init;}
        public List<BoardMemberResponseDto> Members { get; init; }
    };

}
