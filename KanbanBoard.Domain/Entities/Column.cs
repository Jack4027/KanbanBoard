using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KanbanBoard.Domain.Entities
{
    public class Column
    {
        [Key]
        public Guid Id { get; set; }

        [ForeignKey(nameof(Board))]
        public Guid BoardId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        public int Position { get; set; }

        public List<Card> Cards { get; set; } = [];
    }
}
