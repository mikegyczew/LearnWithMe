using LearnWithMe.Domain.Entities;
using LearnWithMe.Infrastructure.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace LearnWithMe.Infrastructure.Tests;

public class SqliteProgressRepositoryTests
{
    [Fact]
    public async Task ProgressAndHistory_ArePersistedAndIsolatedByProfile()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<LearnWithMeDbContext>()
            .UseSqlite(connection)
            .Options;
        var contextFactory = new TestContextFactory(options);
        var profiles = new SqliteChildProfileRepository(contextFactory);
        var progress = new SqliteProgressRepository(contextFactory);
        var firstProfile = new ChildProfile("Ania");
        var secondProfile = new ChildProfile("Olek");

        await profiles.AddAsync(firstProfile);
        await profiles.AddAsync(secondProfile);
        await progress.MarkLearnedAsync(firstProfile.Id, "Ą");
        await progress.RecordPracticeAsync(firstProfile.Id, "Ą", "FindLetter", wasCorrect: true);
        await progress.RecordPracticeAsync(firstProfile.Id, "B", "MatchWord", wasCorrect: false);

        var firstProgress = await progress.GetProgressAsync(firstProfile.Id);
        var history = await progress.GetHistoryAsync(firstProfile.Id);
        var secondProgress = await progress.GetProgressAsync(secondProfile.Id);

        Assert.Equal(2, firstProgress.Count);
        Assert.Contains(firstProgress, item =>
            item.Letter == "Ą" && item.IsLearned && item.PracticeCount == 1 && item.CorrectCount == 1);
        Assert.Contains(firstProgress, item => item.Letter == "B" && !item.IsLearned);
        Assert.Equal(2, history.Count);
        Assert.Empty(secondProgress);
    }

    [Fact]
    public async Task DeleteProfileAsync_DeletesProfileAndAssociatedProgressAndHistory()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<LearnWithMeDbContext>()
            .UseSqlite(connection)
            .Options;
        var contextFactory = new TestContextFactory(options);
        var profiles = new SqliteChildProfileRepository(contextFactory);
        var progress = new SqliteProgressRepository(contextFactory);
        var mathProgress = new SqliteMathProgressRepository(contextFactory);
        var profile = new ChildProfile("Ania");
        var otherProfile = new ChildProfile("Olek");
        await profiles.AddAsync(profile);
        await profiles.AddAsync(otherProfile);
        await progress.MarkLearnedAsync(profile.Id, "A");
        await progress.RecordPracticeAsync(profile.Id, "A", "FindLetter", wasCorrect: true);
        await progress.RecordPracticeAsync(otherProfile.Id, "B", "FindLetter", wasCorrect: true);
        await mathProgress.RecordAnswerAsync(profile.Id, MathTopic.Addition, wasCorrect: true);
        await mathProgress.RecordAnswerAsync(otherProfile.Id, MathTopic.Division, wasCorrect: false);

        await profiles.DeleteAsync(profile.Id);

        Assert.DoesNotContain(await profiles.GetAllAsync(), item => item.Id == profile.Id);
        Assert.Empty(await progress.GetProgressAsync(profile.Id));
        Assert.Empty(await progress.GetHistoryAsync(profile.Id));
        Assert.Empty(await mathProgress.GetProgressAsync(profile.Id));
        Assert.Single(await progress.GetProgressAsync(otherProfile.Id));
        Assert.Single(await progress.GetHistoryAsync(otherProfile.Id));
        Assert.Single(await mathProgress.GetProgressAsync(otherProfile.Id));
    }

    [Fact]
    public async Task MathProgress_IsPersistedAndIsolatedByProfileAndTopic()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<LearnWithMeDbContext>()
            .UseSqlite(connection)
            .Options;
        var contextFactory = new TestContextFactory(options);
        var profiles = new SqliteChildProfileRepository(contextFactory);
        var progress = new SqliteMathProgressRepository(contextFactory);
        var firstProfile = new ChildProfile("Ania");
        var secondProfile = new ChildProfile("Olek");

        await profiles.AddAsync(firstProfile);
        await profiles.AddAsync(secondProfile);
        await progress.RecordAnswerAsync(firstProfile.Id, MathTopic.Addition, wasCorrect: true);
        await progress.RecordAnswerAsync(firstProfile.Id, MathTopic.Addition, wasCorrect: false);
        await progress.RecordAnswerAsync(firstProfile.Id, MathTopic.Division, wasCorrect: true);

        var firstProgress = await progress.GetProgressAsync(firstProfile.Id);

        Assert.Equal(2, firstProgress.Count);
        Assert.Contains(firstProgress, item =>
            item.Topic == MathTopic.Addition && item.Attempts == 2 && item.Correct == 1);
        Assert.Contains(firstProgress, item =>
            item.Topic == MathTopic.Division && item.Attempts == 1 && item.Correct == 1);
        Assert.Empty(await progress.GetProgressAsync(secondProfile.Id));
    }

    [Fact]
    public async Task MathRepository_CreatesProgressTableForAnExistingDatabase()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using (var command = connection.CreateCommand())
        {
            command.CommandText = "CREATE TABLE LegacyMarker (Id INTEGER PRIMARY KEY);";
            await command.ExecuteNonQueryAsync();
        }

        var options = new DbContextOptionsBuilder<LearnWithMeDbContext>()
            .UseSqlite(connection)
            .Options;
        var contextFactory = new TestContextFactory(options);
        var progress = new SqliteMathProgressRepository(contextFactory);
        var profileId = Guid.NewGuid();

        await progress.RecordAnswerAsync(profileId, MathTopic.Multiplication, wasCorrect: true);

        var savedProgress = await progress.GetProgressAsync(profileId);
        var topicProgress = Assert.Single(savedProgress);
        Assert.Equal(MathTopic.Multiplication, topicProgress.Topic);
        Assert.Equal(1, topicProgress.Attempts);
        Assert.Equal(1, topicProgress.Correct);
    }

    private sealed class TestContextFactory(DbContextOptions<LearnWithMeDbContext> options)
        : IDbContextFactory<LearnWithMeDbContext>
    {
        public LearnWithMeDbContext CreateDbContext()
        {
            return new LearnWithMeDbContext(options);
        }

        public Task<LearnWithMeDbContext> CreateDbContextAsync(
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(CreateDbContext());
        }
    }
}
