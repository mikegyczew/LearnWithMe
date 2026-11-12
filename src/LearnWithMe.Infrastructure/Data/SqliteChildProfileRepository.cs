using LearnWithMe.Application.Abstractions;
using LearnWithMe.Domain.Entities;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace LearnWithMe.Infrastructure.Data;

public sealed class SqliteChildProfileRepository(
    IDbContextFactory<LearnWithMeDbContext> contextFactory) : IChildProfileRepository
{
    public async Task<IReadOnlyList<ChildProfile>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
            await context.Database.EnsureCreatedAsync(cancellationToken);

            var profiles = await context.ChildProfiles
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            return profiles
                .OrderBy(profile => profile.CreatedAtUtc)
                .ThenBy(profile => profile.Id)
                .ToArray();
        }
        catch (SqliteException exception)
        {
            throw new ProfileStorageException("Unable to read child profiles from local storage.", exception);
        }
    }

    public async Task AddAsync(ChildProfile profile, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(profile);

        try
        {
            await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
            await context.Database.EnsureCreatedAsync(cancellationToken);
            context.ChildProfiles.Add(profile);
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (SqliteException exception)
        {
            throw new ProfileStorageException("Unable to save a child profile to local storage.", exception);
        }
        catch (DbUpdateException exception) when (exception.InnerException is SqliteException)
        {
            throw new ProfileStorageException("Unable to save a child profile to local storage.", exception);
        }
    }

    public async Task DeleteAsync(Guid profileId, CancellationToken cancellationToken = default)
    {
        if (profileId == Guid.Empty)
        {
            throw new ArgumentException("Profile ID cannot be empty.", nameof(profileId));
        }

        try
        {
            await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
            await context.Database.EnsureCreatedAsync(cancellationToken);
            await MathProgressStorage.EnsureCreatedAsync(context, cancellationToken);
            await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

            var profile = await context.ChildProfiles
                .FirstOrDefaultAsync(item => item.Id == profileId, cancellationToken);
            if (profile is null)
            {
                return;
            }

            var progress = await context.LetterProgress
                .Where(item => item.ProfileId == profileId)
                .ToListAsync(cancellationToken);
            var history = await context.PracticeRecords
                .Where(item => item.ProfileId == profileId)
                .ToListAsync(cancellationToken);
            var mathProgress = await context.MathTopicProgress
                .Where(item => item.ProfileId == profileId)
                .ToListAsync(cancellationToken);

            context.LetterProgress.RemoveRange(progress);
            context.PracticeRecords.RemoveRange(history);
            context.MathTopicProgress.RemoveRange(mathProgress);
            context.ChildProfiles.Remove(profile);
            await context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (SqliteException exception)
        {
            throw new ProfileStorageException("Unable to delete child profile and its learning data.", exception);
        }
        catch (DbUpdateException exception) when (exception.InnerException is SqliteException)
        {
            throw new ProfileStorageException("Unable to delete child profile and its learning data.", exception);
        }
    }
}
