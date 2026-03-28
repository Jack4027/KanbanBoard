namespace KanbanBoard.Application.DTOs.Responses
{
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
