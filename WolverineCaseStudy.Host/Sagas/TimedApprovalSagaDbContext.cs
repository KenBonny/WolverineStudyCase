using Microsoft.EntityFrameworkCore;

namespace WolverineCaseStudy.Host.Sagas;

public class TimedApprovalSagaDbContext(DbContextOptions<TimedApprovalSagaDbContext> options) : DbContext(options)
{
    public DbSet<TimedApprovalSaga> TimedApprovalSagas => Set<TimedApprovalSaga>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TimedApprovalSaga>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Status).HasConversion<string>();
        });

        base.OnModelCreating(modelBuilder);
    }
}

