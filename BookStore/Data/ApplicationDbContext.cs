using BookStore.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Data;

public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Book> Books { get; set; }
    public DbSet<Author> Authors { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Keyword> Keywords { get; set; }

    public DbSet<BookAuthor> BookAuthors { get; set; }
    public DbSet<BookKeyword> BookKeywords { get; set; }
    public DbSet<BookRelation> BookRelations { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // BookAuthor (many-to-many)
        modelBuilder.Entity<BookAuthor>()
            .HasKey(x => new { x.BookId, x.AuthorId });

        modelBuilder.Entity<BookAuthor>()
            .HasOne(x => x.Book)
            .WithMany(b => b.BookAuthors)
            .HasForeignKey(x => x.BookId);

        modelBuilder.Entity<BookAuthor>()
            .HasOne(x => x.Author)
            .WithMany(a => a.BookAuthors)
            .HasForeignKey(x => x.AuthorId);

        // BookKeyword (many-to-many)
        modelBuilder.Entity<BookKeyword>()
            .HasKey(x => new { x.BookId, x.KeywordId });

        modelBuilder.Entity<BookKeyword>()
            .HasOne(x => x.Book)
            .WithMany(b => b.BookKeywords)
            .HasForeignKey(x => x.BookId);

        modelBuilder.Entity<BookKeyword>()
            .HasOne(x => x.Keyword)
            .WithMany(k => k.BookKeywords)
            .HasForeignKey(x => x.KeywordId);

        // BookRelation (self relation)
        modelBuilder.Entity<BookRelation>()
            .HasOne(br => br.Book)
            .WithMany(b => b.RelatedTo)
            .HasForeignKey(br => br.BookId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<BookRelation>()
            .HasOne(br => br.RelatedBook)
            .WithMany(b => b.RelatedFrom)
            .HasForeignKey(br => br.RelatedBookId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    // ✅ Timestamp handling
    public override int SaveChanges()
    {
        SetTimestamps();
        return base.SaveChanges();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SetTimestamps();
        return await base.SaveChangesAsync(cancellationToken);
    }

    private void SetTimestamps()
    {
        var entries = ChangeTracker.Entries<BaseEntity>();

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = DateTime.UtcNow;
                entry.Entity.UpdatedAt = DateTime.UtcNow;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTime.UtcNow;
            }
        }
    }
}