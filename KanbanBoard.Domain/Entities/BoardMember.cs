using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KanbanBoard.Domain.Entities
{
    //Represents the members of a board, it has the board id, user id and the role of the member in the board (owner, admin or member)
    //Helps implement a many to many relationship between the board and the user, as a user can be a member of many boards and a board can have many members
    
    public class BoardMember
    {
        //Use a composite key of BoardId and UserId to ensure that a user can only be a member of a board once
        [ForeignKey(nameof(Board))]
        public Guid BoardId { get; set; }

        //User Id is a string because we are using Identity for authentication and the user id is a string in Identity
        [Required]
        public string UserId { get; set; } = string.Empty;

        //The role of the member in the board, it can be owner, admin or member, this is used to determine the permissions of the member in the board
        //Not using identity roles because the roles are specific to the board and not global, a user can be an owner in one board and a member in another board
        [Required]
        public string Role { get; set; } = string.Empty;
    }
}
