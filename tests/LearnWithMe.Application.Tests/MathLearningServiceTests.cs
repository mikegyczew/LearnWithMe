using System.Text.RegularExpressions;
using LearnWithMe.Application.Abstractions;
using LearnWithMe.Application.Features.Math;
using LearnWithMe.Domain.Entities;

namespace LearnWithMe.Application.Tests;

public class MathLearningServiceTests
{
    [Theory]
    [InlineData(MathTopic.Numbers)]
    [InlineData(MathTopic.Counting)]
    [InlineData(MathTopic.Addition)]
    [InlineData(MathTopic.Subtraction)]
    [InlineData(MathTopic.Multiplication)]
    [InlineData(MathTopic.Division)]
    public void CreateQuestion_UsesFirstGradeRangeAndIncludesOneCorrectOption(MathTopic topic)
    {
        var service = new MathLearningService(new InMemoryMathProgressRepository());

        for (var index = 0; index < 100; index++)
        {
            var question = service.CreateQuestion(topic);

            Assert.Equal(topic, question.Topic);
            Assert.InRange(question.CorrectAnswer, 0, 20);
            Assert.Equal(4, question.Options.Count);
            Assert.Equal(4, question.Options.Distinct().Count());
            Assert.All(question.Options, option => Assert.InRange(option, 0, 20));
            Assert.Contains(question.CorrectAnswer, question.Options);
            Assert.False(string.IsNullOrWhiteSpace(question.Prompt));
            if (topic != MathTopic.Numbers)
            {
                Assert.False(string.IsNullOrWhiteSpace(question.VisualHint));
            }
        }
    }

    [Theory]
    [InlineData(MathTopic.Addition)]
    [InlineData(MathTopic.Subtraction)]
    [InlineData(MathTopic.Multiplication)]
    [InlineData(MathTopic.Division)]
    public void CreateQuestion_ProducesCorrectArithmetic(MathTopic topic)
    {
        var service = new MathLearningService(new InMemoryMathProgressRepository());

        for (var index = 0; index < 100; index++)
        {
            var question = service.CreateQuestion(topic);
            var numbers = Regex.Matches(question.Prompt, @"\d+")
                .Select(match => int.Parse(match.Value))
                .ToArray();

            var expected = topic switch
            {
                MathTopic.Addition => numbers[0] + numbers[1],
                MathTopic.Subtraction => numbers[0] - numbers[1],
                MathTopic.Multiplication => numbers[0] * numbers[1],
                MathTopic.Division => numbers[0] / numbers[1],
                _ => throw new ArgumentOutOfRangeException(nameof(topic))
            };

            Assert.Equal(expected, question.CorrectAnswer);
        }
    }

    [Fact]
    public void NumberQuestions_IncludeSpokenNumberAndCorrectObjectForms()
    {
        var service = new MathLearningService(new InMemoryMathProgressRepository());
        for (var index = 0; index < 200; index++)
        {
            var question = service.CreateQuestion(MathTopic.Numbers);
            Assert.StartsWith($"Znajdź liczbę {question.CorrectAnswer}.", question.Prompt);
            Assert.False(string.IsNullOrWhiteSpace(question.NumberName));
            Assert.Equal(3, question.ReadingExamples.Count);
            var endings = question.CorrectAnswer == 1
                ? new[] { "samochód", "sukienka", "zabawka" }
                : question.CorrectAnswer % 10 is >= 2 and <= 4 &&
                    question.CorrectAnswer % 100 is not (>= 12 and <= 14)
                    ? new[] { "samochody", "sukienki", "zabawki" }
                    : new[] { "samochodów", "sukienek", "zabawek" };
            var prefixes = question.CorrectAnswer == 1
                ? new[] { "jeden", "jedna", "jedna" }
                : question.CorrectAnswer == 2
                    ? new[] { "dwa", "dwie", "dwie" }
                    : new[] { question.NumberName, question.NumberName, question.NumberName };
            for (var itemIndex = 0; itemIndex < question.ReadingExamples.Count; itemIndex++)
            {
                Assert.Equal(
                    $"{prefixes[itemIndex]} {endings[itemIndex]}",
                    question.ReadingExamples[itemIndex].Phrase);
            }
        }
    }

    [Fact]
    public async Task RecordAnswerAsync_StoresTheSelectedTopicAndOutcome()
    {
        var repository = new InMemoryMathProgressRepository();
        var service = new MathLearningService(repository);
        var profileId = Guid.NewGuid();

        await service.RecordAnswerAsync(profileId, MathTopic.Addition, wasCorrect: true);

        Assert.Equal((profileId, MathTopic.Addition, true), repository.LastAnswer);
    }

    private sealed class InMemoryMathProgressRepository : IMathProgressRepository
    {
        public (Guid ProfileId, MathTopic Topic, bool WasCorrect)? LastAnswer { get; private set; }

        public Task<IReadOnlyList<MathTopicProgress>> GetProgressAsync(
            Guid profileId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<MathTopicProgress>>([]);
        }

        public Task RecordAnswerAsync(
            Guid profileId,
            MathTopic topic,
            bool wasCorrect,
            CancellationToken cancellationToken = default)
        {
            LastAnswer = (profileId, topic, wasCorrect);
            return Task.CompletedTask;
        }
    }
}
