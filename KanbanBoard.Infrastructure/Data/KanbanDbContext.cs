using KanbanBoard.Domain.Entities;
using KanbanBoard.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace KanbanBoard.Infrastructure.Data
{
    public class KanbanDbContext(DbContextOptions<KanbanDbContext> options)
        : IdentityDbContext<AppUserIdentity>(options)
    {
        public DbSet<Board> Boards => Set<Board>();
        public DbSet<Column> Columns => Set<Column>();
        public DbSet<Card> Cards => Set<Card>();
        public DbSet<BoardMember> BoardMembers => Set<BoardMember>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Board>(entity =>
            {
                entity.HasKey(b => b.Id);
                entity.Property(b => b.Name).IsRequired().HasMaxLength(100);
                entity.Property(b => b.CreatedBy).IsRequired();
                entity.HasMany(b => b.Columns)
                    .WithOne()
                    .HasForeignKey(c => c.BoardId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasMany(b => b.Members)
                    .WithOne()
                    .HasForeignKey(m => m.BoardId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Column>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Name).IsRequired().HasMaxLength(100);
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
                entity.HasKey(m => new { m.BoardId, m.UserId });
                entity.Property(m => m.Role).IsRequired();
            });
        }
    }
}
