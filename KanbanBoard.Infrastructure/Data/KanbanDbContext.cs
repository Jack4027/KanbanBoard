using KanbanBoard.Domain.Entities;
using KanbanBoard.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace KanbanBoard.Infrastructure.Data
{
    // The KanbanDbContext class is responsible for managing the database context for the Kanban board application.
    // It inherits from IdentityDbContext to integrate ASP.NET Core Identity for user authentication and authorization.
    public class KanbanDbContext(DbContextOptions<KanbanDbContext> options)
        : IdentityDbContext<AppUserIdentity>(options)
    {
        //Creaing data sets for the entities in the application, including Board, Column, Card and BoardMember.
        //These properties allow for querying and manipulating the corresponding tables in the database using Entity Framework Core's DbSet API.
        public DbSet<Board> Boards => Set<Board>();
        public DbSet<Column> Columns => Set<Column>();
        public DbSet<Card> Cards => Set<Card>();
        public DbSet<BoardMember> BoardMembers => Set<BoardMember>();

        //Configuring the entity relationships and constraints using the Fluent API in the OnModelCreating method.
        //This includes defining primary keys, required properties, maximum lengths for string properties, and cascade delete behaviors for related entities.
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Call the base implementation to ensure that the IdentityDbContext configurations are applied correctly, including the setup of ASP.NET Core Identity tables and relationships.
            base.OnModelCreating(modelBuilder);


            modelBuilder.Entity<Board>(entity =>
            {
                entity.HasKey(b => b.Id);
                entity.Property(b => b.Name).IsRequired().HasMaxLength(100);
                entity.Property(b => b.CreatedBy).IsRequired();
                //Map many columsn to 1 board using fluent api, and set cascade delete behavior so that when a board is deleted, all associated columns and members are also deleted from the database, ensuring data integrity and preventing orphaned records.
                entity.HasMany(b => b.Columns)
                    .WithOne()
                    .HasForeignKey(c => c.BoardId)
                    .OnDelete(DeleteBehavior.Cascade);
                //A board can have any board members, and when a board is deleted, all associated members are also deleted from the database, ensuring data integrity and preventing orphaned records.
                entity.HasMany(b => b.Members)
                    .WithOne()
                    .HasForeignKey(m => m.BoardId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Column>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Name).IsRequired().HasMaxLength(100);
                //Map that a column can have many cards using fluent api, and set cascade delete behavior so that when a column is deleted, all associated cards are also deleted from the database, ensuring data integrity and preventing orphaned records.
                entity.HasMany(c => c.Cards)
                    .WithOne()
                    .HasForeignKey(c => c.ColumnId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Card>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Title).IsRequired().HasMaxLength(200);
                entity.Property(c => c.Description).HasMaxLength(2000);
            });

            modelBuilder.Entity<BoardMember>(entity =>
            {
                //The composite key here permits that a board can have many users and a user can be a member of many boards, while ensuring that each combination of BoardId and UserId is unique within the BoardMembers table.
                entity.HasKey(m => new { m.BoardId, m.UserId });
                //Board members must have a role assigned to them, and this property is required to ensure that every member has a defined role within the board, which can be used for authorization and access control
                entity.Property(m => m.Role).IsRequired();
            });
        }
    }
}
