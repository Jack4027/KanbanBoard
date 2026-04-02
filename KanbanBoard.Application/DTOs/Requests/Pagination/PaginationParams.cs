using System;
using System.Collections.Generic;
using System.Text;

namespace KanbanBoard.Application.DTOs.Requests.Pagination
{
    //This object represents the paganation parameters passed by the user in the query string, it has default values and also limits the maximum page size to prevent abuse
    public class PaginationParams
    {
        private const int MaxPageSize = 50;
        private int _pageSize = 10;

        public int Page { get; set; } = 1;

        //Custom setter to implement the maximum page size limit
        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = value > MaxPageSize ? MaxPageSize : value;
        }
    }
}
