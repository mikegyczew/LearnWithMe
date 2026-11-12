using LearnWithMe.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LearnWithMe.Infrastructure.Data;

public sealed class LearnWithMeDbContext(DbContextOptions<LearnWithMeDbContext> options)
    : DbContext(options)
{
    public DbSet<ChildProfile> ChildProfiles => Set<ChildProfile>();

    public DbSet<LetterProgress> LetterProgress => Set<LetterProgress>();

    public DbSet<PracticeRecord> PracticeRecords => Set<PracticeRecord>();

    public DbSet<MathTopicProgress> MathTopicProgress => Set<MathTopicProgress>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ChildProfile>(entity =>
        {
            entity.HasKey(profile => profile.Id);
            entity.Property(profile => profile.Name).HasMaxLength(30).IsRequired();
            entity.Property(profile => profile.Avatar).HasMaxLength(32);
            entity.Property(profile => profile.CreatedAtUtc).IsRequired();
        });

        modelBuilder.Entity<LetterProgress>(entity =>
        {
            entity.HasKey(progress => new { progress.ProfileId, progress.Letter });
            entity.Property(progress => progress.Letter).HasMaxLength(2).IsRequired();
            entity.Property(progress => progress.LastPracticedAtUtc).IsRequired();
        });

        modelBuilder.Entity<PracticeRecord>(entity =>
        {
            entity.HasKey(record => record.Id);
            entity.Property(record => record.Letter).HasMaxLength(2).IsRequired();
            entity.Property(record => record.ExerciseType).HasMaxLength(32).IsRequired();
            entity.Property(record => record.CreatedAtUtc).IsRequired();
            entity.HasIndex(record => new { record.ProfileId, record.CreatedAtUtc });
        });

        modelBuilder.Entity<MathTopicProgress>(entity =>
        {
            entity.HasKey(progress => new { progress.ProfileId, progress.Topic });
            entity.Property(progress => progress.Topic).HasConversion<int>();
        });
    }
}
