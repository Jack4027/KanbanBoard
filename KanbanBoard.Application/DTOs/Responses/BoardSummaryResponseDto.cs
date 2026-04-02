using System;
using System.Collections.Generic;
using System.Text;

namespace KanbanBoard.Application.DTOs.Responses
{
    //Object sent back to angular to represent the summary of a board, including the number of columns and members in the board,
    //this is used in the board list page to show the boards in a summarized way without sending all the details of the board
    public record BoardSummaryResponseDto
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public string CreatedBy { get; init; } = string.Empty;
        public DateTime CreatedAt { get; init; }
        public int ColumnCount { get; init; }
        public int MemberCount { get; init; }
    }

}
