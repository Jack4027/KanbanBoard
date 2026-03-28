using System;
using System.Collections.Generic;
using System.Text;

namespace KanbanBoard.Application.DTOs.Responses
{
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
