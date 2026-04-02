using System;
using System.Collections.Generic;
using System.Text;

namespace KanbanBoard.Application.DTOs.Responses.Pagination
{
    //Represents the object returned back to the client, that includes the paginated items and also the pagination metadata such as total count, current page, page size and total pages
    public class PagedResult<T>
    {
        public IEnumerable<T> Items { get; set; } = [];
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

    }
}
