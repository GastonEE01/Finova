using Finova.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Finova.Infrastructure.Persistence;

public class FinovaDbContext : DbContext
{
    public FinovaDbContext(DbContextOptions<FinovaDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Movement> Movements => Set<Movement>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(e =>
        {
            e.HasKey(u => u.Id);
            e.Property(u => u.Email).IsRequired().HasMaxLength(255);
            e.HasIndex(u => u.Email).IsUnique();
            e.Property(u => u.CreatedAt).IsRequired();
        });

        modelBuilder.Entity<Account>(e =>
        {
            e.HasKey(a => a.Id);
            e.Property(a => a.Name).IsRequired().HasMaxLength(100);
            e.Property(a => a.Currency).IsRequired().HasMaxLength(3);
            e.HasIndex(a => a.UserId);
            e.HasOne(a => a.User)
             .WithMany(u => u.Accounts)
             .HasForeignKey(a => a.UserId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Category>(e =>
        {
            e.HasKey(c => c.Id);
            e.Property(c => c.Name).IsRequired().HasMaxLength(100);
            e.Property(c => c.Type).IsRequired();
            e.HasIndex(c => c.UserId);
            e.HasOne(c => c.User)
             .WithMany(u => u.Categories)
             .HasForeignKey(c => c.UserId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Movement>(e =>
        {
            e.HasKey(m => m.Id);
            e.Property(m => m.Type).IsRequired();
            e.Property(m => m.Amount).IsRequired().HasPrecision(18, 2);
            e.Property(m => m.Date).IsRequired();
            e.Property(m => m.Description).HasMaxLength(500);
            e.HasIndex(m => m.AccountId);
            e.HasIndex(m => m.Date);
            e.HasOne(m => m.Account)
             .WithMany(a => a.Movements)
             .HasForeignKey(m => m.AccountId)
             .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(m => m.Category)
             .WithMany(c => c.Movements)
             .HasForeignKey(m => m.CategoryId)
             .OnDelete(DeleteBehavior.SetNull);
        });
    }
}
