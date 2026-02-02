using Microsoft.EntityFrameworkCore;
using Leximi.Domain.Entities;
using Leximi.Domain.Common;

namespace Leximi.Infrastructure.Persistence;

public class LeximiDbContext : DbContext
{
    public LeximiDbContext(DbContextOptions<LeximiDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<LearningSet> LearningSets => Set<LearningSet>();
    public DbSet<LearningItem> LearningItems => Set<LearningItem>();
    public DbSet<Question> Questions => Set<Question>();
    public DbSet<Answer> Answers => Set<Answer>();
    public DbSet<Flashcard> Flashcards => Set<Flashcard>();
    public DbSet<LearningSetAttempt> Attempts => Set<LearningSetAttempt>();
    public DbSet<UserAnswer> UserAnswers => Set<UserAnswer>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        

        
        // Apply configurations from assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(LeximiDbContext).Assembly);

        // Disable cascade delete for all relationships (use soft delete instead)
        foreach (var relationship in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
        {
            relationship.DeleteBehavior = DeleteBehavior.Restrict;
        }
        
        // Default filters for entities inheriting from BaseEntity
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(ConvertFilterExpression(entityType.ClrType));
            }
        }
    }

    private static System.Linq.Expressions.LambdaExpression ConvertFilterExpression(Type type)
    {
        var parameter = System.Linq.Expressions.Expression.Parameter(type, "e");
        var property = System.Linq.Expressions.Expression.Property(parameter, nameof(BaseEntity.IsDeleted));
        var falseConstant = System.Linq.Expressions.Expression.Constant(false);
        var body = System.Linq.Expressions.Expression.Equal(property, falseConstant);
        return System.Linq.Expressions.Expression.Lambda(body, parameter);
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
