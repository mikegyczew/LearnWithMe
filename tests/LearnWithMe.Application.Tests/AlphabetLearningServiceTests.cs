using LearnWithMe.Application.Abstractions;
using LearnWithMe.Application.Features.Alphabet;
using LearnWithMe.Domain.Entities;

namespace LearnWithMe.Application.Tests;

public class AlphabetLearningServiceTests
{
    [Fact]
    public void AlphabetContainsAllThirtyTwoPolishLetters()
    {
        Assert.Equal(32, AlphabetLetter.All.Count);
        Assert.Equal(32, AlphabetLetter.All.Select(letter => letter.Uppercase).Distinct().Count());
        Assert.Contains(AlphabetLetter.All, letter => letter.Uppercase == "Ą");
        Assert.Contains(AlphabetLetter.All, letter => letter.Uppercase == "Ę");
        Assert.Contains(AlphabetLetter.All, letter => letter.Uppercase == "Ł");
        Assert.Contains(AlphabetLetter.All, letter => letter.Uppercase == "Ń");
        Assert.Contains(AlphabetLetter.All, letter => letter.Uppercase == "Ó");
        Assert.Contains(AlphabetLetter.All, letter => letter.Uppercase == "Ś");
        Assert.Contains(AlphabetLetter.All, letter => letter.Uppercase == "Ź");
        Assert.Contains(AlphabetLetter.All, letter => letter.Uppercase == "Ż");
    }

    [Theory]
    [InlineData(ExerciseType.FindLetter)]
    [InlineData(ExerciseType.MatchWord)]
    [InlineData(ExerciseType.FindFirstLetter)]
    [InlineData(ExerciseType.MatchCase)]
    public void CreateQuestion_IncludesCorrectAnswerAndFourDistinctOptions(ExerciseType type)
    {
        var service = new AlphabetLearningService(new InMemoryProgressRepository());

        for (var index = 0; index < 100; index++)
        {
            var question = service.CreateQuestion(type);

            Assert.Equal(type, question.Type);
            Assert.Equal(4, question.Options.Count);
            Assert.Equal(4, question.Options.Distinct().Count());
            Assert.Contains(question.CorrectAnswer, question.Options);
            Assert.Contains(AlphabetLetter.All, letter => letter.Uppercase == question.Letter);
            Assert.False(string.IsNullOrWhiteSpace(question.Prompt));
        }
    }

    [Fact]
    public async Task RecordAnswerAsync_StoresTheQuestionOutcome()
    {
        var repository = new InMemoryProgressRepository();
        var service = new AlphabetLearningService(repository);
        var question = service.CreateQuestion(ExerciseType.FindLetter);
        var profileId = Guid.NewGuid();

        await service.RecordAnswerAsync(profileId, question, wasCorrect: true);

        Assert.Equal((profileId, question.Letter, "FindLetter", true), repository.LastPractice);
    }

    private sealed class InMemoryProgressRepository : IProgressRepository
    {
        public (Guid ProfileId, string Letter, string Type, bool WasCorrect)? LastPractice { get; private set; }

        public Task<IReadOnlyList<LetterProgress>> GetProgressAsync(
            Guid profileId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<LetterProgress>>([]);
        }

        public Task<IReadOnlyList<PracticeRecord>> GetHistoryAsync(
            Guid profileId,
            int limit = 20,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<PracticeRecord>>([]);
        }

        public Task MarkLearnedAsync(
            Guid profileId,
            string letter,
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task RecordPracticeAsync(
            Guid profileId,
            string letter,
            string exerciseType,
            bool wasCorrect,
            CancellationToken cancellationToken = default)
        {
            LastPractice = (profileId, letter, exerciseType, wasCorrect);
            return Task.CompletedTask;
        }
    }
}
