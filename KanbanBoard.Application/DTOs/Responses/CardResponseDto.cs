namespace KanbanBoard.Application.DTOs.Responses
{
    public record CardResponseDto
    {
        public Guid Id { get; init; }
        public Guid ColumnId { get; init; }
        public string Title { get; init; }  = string.Empty;
        public string? Description { get; init; }
        public DateTime CreatedAt { get; init; }
    }

}
