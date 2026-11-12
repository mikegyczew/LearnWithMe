using LearnWithMe.Application.Abstractions;
using LearnWithMe.Domain.Entities;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace LearnWithMe.Infrastructure.Data;

public sealed class SqliteProgressRepository(
    IDbContextFactory<LearnWithMeDbContext> contextFactory) : IProgressRepository
{
    public async Task<IReadOnlyList<LetterProgress>> GetProgressAsync(
        Guid profileId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
            await context.Database.EnsureCreatedAsync(cancellationToken);
            return await context.LetterProgress
                .AsNoTracking()
                .Where(progress => progress.ProfileId == profileId)
                .OrderBy(progress => progress.Letter)
                .ToArrayAsync(cancellationToken);
        }
        catch (SqliteException exception)
        {
            throw new ProfileStorageException("Unable to read learning progress from local storage.", exception);
        }
    }

    public async Task<IReadOnlyList<PracticeRecord>> GetHistoryAsync(
        Guid profileId,
        int limit = 20,
        CancellationToken cancellationToken = default)
    {
        if (limit is < 1 or > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(limit), "History limit must be between 1 and 100.");
        }

        try
        {
            await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
            await context.Database.EnsureCreatedAsync(cancellationToken);
            return await context.PracticeRecords
                .AsNoTracking()
                .Where(record => record.ProfileId == profileId)
                .OrderByDescending(record => record.CreatedAtUtc)
                .Take(limit)
                .ToArrayAsync(cancellationToken);
        }
        catch (SqliteException exception)
        {
            throw new ProfileStorageException("Unable to read exercise history from local storage.", exception);
        }
    }

    public async Task MarkLearnedAsync(
        Guid profileId,
        string letter,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
            await context.Database.EnsureCreatedAsync(cancellationToken);
            var progress = await GetOrCreateProgressAsync(context, profileId, letter, cancellationToken);
            progress.MarkLearned();
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (SqliteException exception)
        {
            throw new ProfileStorageException("Unable to save learning progress.", exception);
        }
        catch (DbUpdateException exception) when (exception.InnerException is SqliteException)
        {
            throw new ProfileStorageException("Unable to save learning progress.", exception);
        }
    }

    public async Task RecordPracticeAsync(
        Guid profileId,
        string letter,
        string exerciseType,
        bool wasCorrect,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
            await context.Database.EnsureCreatedAsync(cancellationToken);
            var progress = await GetOrCreateProgressAsync(context, profileId, letter, cancellationToken);
            progress.RecordPractice(wasCorrect);
            context.PracticeRecords.Add(new PracticeRecord(profileId, letter, exerciseType, wasCorrect));
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (SqliteException exception)
        {
            throw new ProfileStorageException("Unable to save exercise progress.", exception);
        }
        catch (DbUpdateException exception) when (exception.InnerException is SqliteException)
        {
            throw new ProfileStorageException("Unable to save exercise progress.", exception);
        }
    }

    private static async Task<LetterProgress> GetOrCreateProgressAsync(
        LearnWithMeDbContext context,
        Guid profileId,
        string letter,
        CancellationToken cancellationToken)
    {
        var progress = await context.LetterProgress.FindAsync(
            [profileId, letter],
            cancellationToken);

        if (progress is not null)
        {
            return progress;
        }

        progress = new LetterProgress(profileId, letter);
        context.LetterProgress.Add(progress);
        return progress;
    }
}
