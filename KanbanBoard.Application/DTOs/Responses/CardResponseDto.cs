namespace KanbanBoard.Application.DTOs.Responses
{
    //Object sent back to angular to represent a card, it includes the card details such as id, title, description and creation date
    public record CardResponseDto
    {
        public Guid Id { get; init; }
        public Guid ColumnId { get; init; }
        public string Title { get; init; }  = string.Empty;
        public string? Description { get; init; }
        public DateTime CreatedAt { get; init; }
    }

}
