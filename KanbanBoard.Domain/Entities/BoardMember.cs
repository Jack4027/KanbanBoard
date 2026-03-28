using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KanbanBoard.Domain.Entities
{
    public class BoardMember
    {
        [ForeignKey(nameof(Board))]
        public Guid BoardId { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = string.Empty;
    }
}
