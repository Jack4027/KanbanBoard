using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KanbanBoard.Domain.Entities
{
    /// Represents a card in the Kanban board, it has a title, description, creation date and a foreign key to the column it belongs to
    public class Card
    {
        // The unique identifier of the card
        [Key]
        public Guid Id { get; set; }

        // The foreign key to the column that the card belongs to
        [ForeignKey(nameof(Column))]
        public Guid ColumnId { get; set; }

        // The navigation property to the column that the card belongs to
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        // The description of the card, it is optional and has a maximum length of 2000 characters
        [MaxLength(2000)]
        public string? Description { get; set; }

        // The creation date of the card, it is set to the current date and time when the card is created
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
