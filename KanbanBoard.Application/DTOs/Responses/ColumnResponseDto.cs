namespace KanbanBoard.Application.DTOs.Responses
{
    public record ColumnResponseDto
    {
        public Guid Id { get; init; }
        public Guid BoardId { get; init; }
        public string Name { get; init; } = string.Empty;
        public int Position { get; init; }
        public List<CardResponseDto> Cards { get; init; } = [];
    }

}
