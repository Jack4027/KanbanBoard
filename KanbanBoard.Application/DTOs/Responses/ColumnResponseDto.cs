namespace KanbanBoard.Application.DTOs.Responses
{
    //Object sent back to angular to represent a column, it includes the column details such as id, name and position, as well as the cards in the column
    public record ColumnResponseDto
    {
        public Guid Id { get; init; }
        public Guid BoardId { get; init; }
        public string Name { get; init; } = string.Empty;
        public int Position { get; init; }
        public List<CardResponseDto> Cards { get; init; } = [];
    }

}
