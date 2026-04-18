using Microsoft.EntityFrameworkCore;
using Leximi.Domain.Entities;
using Leximi.Domain.Common;

namespace Leximi.Infrastructure.Persistence;

public class LeximiDbContext(DbContextOptions<LeximiDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<LearningSet> LearningSets => Set<LearningSet>();
    public DbSet<LearningItem> LearningItems => Set<LearningItem>();
    public DbSet<Answer> Answers => Set<Answer>();
    public DbSet<LearningSetAttempt> Attempts => Set<LearningSetAttempt>();
    public DbSet<UserAnswer> UserAnswers => Set<UserAnswer>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        

        
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(LeximiDbContext).Assembly);

        // Disable cascade delete globally to prevent cycles in SQL Server
        foreach (var relationship in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
        {
            relationship.DeleteBehavior = DeleteBehavior.Restrict;
        }

        modelBuilder.Entity<LearningSet>()
            .HasMany(s => s.Items)
            .WithOne(i => i.LearningSet)
            .HasForeignKey(i => i.LearningSetId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<LearningItem>()
            .HasMany(i => i.Answers)
            .WithOne(a => a.LearningItem)
            .HasForeignKey(a => a.LearningItemId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    break;
                case EntityState.Modified:
                    entry.Entity.ModifiedAt = DateTime.UtcNow;
                    break;
                case EntityState.Deleted:
                    entry.State = EntityState.Modified;
                    entry.Entity.IsDeleted = true;
                    break;
            }
        }
        return base.SaveChangesAsync(cancellationToken);
    }
}
