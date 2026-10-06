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
    public DbSet<Budget> Budgets => Set<Budget>();
    public DbSet<SavingGoal> SavingGoals => Set<SavingGoal>();

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
            e.HasIndex(m => m.GoalId);
            e.HasOne(m => m.SavingGoal)
             .WithMany(g => g.Contributions)
             .HasForeignKey(m => m.GoalId)
             .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Budget>(e =>
        {
            e.HasKey(b => b.Id);
            e.Property(b => b.Amount).IsRequired().HasPrecision(18, 2);
            e.Property(b => b.Currency).IsRequired().HasMaxLength(3);
            e.HasIndex(b => new { b.UserId, b.CategoryId, b.Year, b.Month, b.Currency }).IsUnique();
            e.HasIndex(b => new { b.UserId, b.Year, b.Month });
            e.HasOne(b => b.User)
             .WithMany(u => u.Budgets)
             .HasForeignKey(b => b.UserId)
             .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(b => b.Category)
             .WithMany(c => c.Budgets)
             .HasForeignKey(b => b.CategoryId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<SavingGoal>(e =>
        {
            e.HasKey(g => g.Id);
            e.Property(g => g.Name).IsRequired().HasMaxLength(100);
            e.Property(g => g.TargetAmount).IsRequired().HasPrecision(18, 2);
            e.Property(g => g.TargetDate).IsRequired();
            e.Property(g => g.CreatedAt).IsRequired();
            e.HasIndex(g => g.UserId);
            e.HasOne(g => g.User)
             .WithMany(u => u.SavingGoals)
             .HasForeignKey(g => g.UserId)
             .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
