using LearnWithMe.Application.Abstractions;
using LearnWithMe.Domain.Entities;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace LearnWithMe.Infrastructure.Data;

public sealed class SqliteMathProgressRepository(
    IDbContextFactory<LearnWithMeDbContext> contextFactory) : IMathProgressRepository
{
    public async Task<IReadOnlyList<MathTopicProgress>> GetProgressAsync(
        Guid profileId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
            await context.Database.EnsureCreatedAsync(cancellationToken);
            await MathProgressStorage.EnsureCreatedAsync(context, cancellationToken);
            return await context.MathTopicProgress
                .AsNoTracking()
                .Where(progress => progress.ProfileId == profileId)
                .OrderBy(progress => progress.Topic)
                .ToArrayAsync(cancellationToken);
        }
        catch (SqliteException exception)
        {
            throw new ProfileStorageException("Unable to read math progress from local storage.", exception);
        }
    }

    public async Task RecordAnswerAsync(
        Guid profileId,
        MathTopic topic,
        bool wasCorrect,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
            await context.Database.EnsureCreatedAsync(cancellationToken);
            await MathProgressStorage.EnsureCreatedAsync(context, cancellationToken);
            var progress = await context.MathTopicProgress.FindAsync(
                [profileId, topic],
                cancellationToken);
            if (progress is null)
            {
                progress = new MathTopicProgress(profileId, topic);
                context.MathTopicProgress.Add(progress);
            }

            progress.RecordAnswer(wasCorrect);
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (SqliteException exception)
        {
            throw new ProfileStorageException("Unable to save math progress.", exception);
        }
        catch (DbUpdateException exception) when (exception.InnerException is SqliteException)
        {
            throw new ProfileStorageException("Unable to save math progress.", exception);
        }
    }
}
