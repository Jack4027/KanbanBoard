using KanbanBoard.Domain.Interfaces.Repository;
using KanbanBoard.Domain.Entities;
using KanbanBoard.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace KanbanBoard.Infrastructure.Repository
{
    //All database operations associated with boards , such as creating, retrieving, updating, and deleting boards, as well as checking for the existence of columns with a specific name on a board.
    public class BoardRepository(KanbanDbContext context) : IBoardRepository
    {
        //Add a new board to the database
        public async Task<Board> Add(Board board)
        {
            context.Boards.Add(board);
            await context.SaveChangesAsync();
            return board;
        }

        //Retrieve a board by its unique identifier, including its columns (ordered by position) and members.
        public async Task<Board?> GetById(Guid id)
        {
            return await context.Boards
                .Include(b => b.Columns.OrderBy(c => c.Position))
                    .ThenInclude(c => c.Cards.OrderBy(card => card.CreatedAt))
                .Include(b => b.Members)
                .FirstOrDefaultAsync(b => b.Id == id);
        }


        //Retrieve all boards cnnected to an associated user Id, implements pagination so that for large amounts of data this is segmented in pages, limiting the total data retrieved per request and improving performance,
        //the total count of items is also returned to allow clients to calculate the total number of pages available based on the page size.
        public async Task<(IEnumerable<Board> Items, int TotalCount)> GetByUserId(string userId, int page, int pageSize)
        {
            //Query to retrieve boards where the user is either the creator or a member, including related columns and members, and ordered by creation date.
            var query = context.Boards
                .Include(b => b.Columns.OrderBy(c => c.Position))
                .Include(b => b.Members)
                .Where(b => b.CreatedBy == userId ||
                            b.Members.Any(m => m.UserId == userId));

            //Calculate the total count of items matching the query before applying pagination, allowing clients to understand the total number of items available for pagination.
            var totalCount = await query.CountAsync();

            //Apply pagination to the query by skipping a calculated number of items based on the current page and page size, and then taking a limited number of items for the current page. The results are ordered by creation date.
            var items = await query
                .OrderBy(b => b.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            //Return the paginated list of boards along with the total count of items matching the query, allowing clients to display the current page of results and understand the total number of items available for pagination.
            return (items, totalCount);
        }

        //Update an existing board's details in the database, such as its name or other properties, and save the changes.
        public async Task<Board> Update(Board board)
        {
            context.Boards.Update(board);
            await context.SaveChangesAsync();
            return board;
        }

        //Delete a board from the database based on its unique identifier, and return a boolean indicating whether the deletion was successful (i.e., if any rows were affected).
        public async Task<bool> Delete(Guid id)
        {
            var rowsAffected = await context.Boards
                .Where(b => b.Id == id)
                .ExecuteDeleteAsync();
            return rowsAffected > 0;
        }

    }
}
