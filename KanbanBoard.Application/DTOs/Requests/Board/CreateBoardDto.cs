using System;
using System.Collections.Generic;
using System.Text;

namespace KanbanBoard.Application.DTOs.Requests.Board
{
    //Represents the user request data to create a board
    public record CreateBoardDto(string Name);
}
