using System;
using System.Collections.Generic;
using System.Text;

namespace KanbanBoard.Application.DTOs.Requests
{
    public record CreateBoardDto(string Name);

    public record UpdateBoardDto(string Name);

    public record CreateColumnDto(string Name);

    public record UpdateColumnDto(string Name);

    public record CreateCardDto(string Title, string? Description);

    public record UpdateCardDto(string Title, string? Description);

    public record MoveCardDto(Guid TargetColumnId);
}
