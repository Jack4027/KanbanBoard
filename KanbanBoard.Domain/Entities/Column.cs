using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KanbanBoard.Domain.Entities
{
    /// Represents a column in the kanban board, it has a name, a position and a list of cards that belong to it. It also has a foreign key to the board it belongs to.
    public class Column
    {
        // The unique identifier of the column
        [Key]
        public Guid Id { get; set; }

        // The foreign key to the board that this column belongs to
        [ForeignKey(nameof(Board))]
        public Guid BoardId { get; set; }

        // The column name
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        // The position of the column in the board, it is used to order the columns in the board
        public int Position { get; set; }

        // The Cards that belong to this column, it is a navigation property that allows us to access the cards of the column when we load the column from the database
        public List<Card> Cards { get; set; } = [];
    }
}
