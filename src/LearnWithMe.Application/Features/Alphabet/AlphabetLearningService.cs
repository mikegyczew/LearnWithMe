using LearnWithMe.Application.Abstractions;
using LearnWithMe.Domain.Entities;

namespace LearnWithMe.Application.Features.Alphabet;

public sealed class AlphabetLearningService(IProgressRepository progressRepository)
    : IAlphabetLearningService
{
    public Task<IReadOnlyList<LetterProgress>> GetProgressAsync(
        Guid profileId,
        CancellationToken cancellationToken = default)
    {
        return progressRepository.GetProgressAsync(profileId, cancellationToken);
    }

    public Task<IReadOnlyList<PracticeRecord>> GetHistoryAsync(
        Guid profileId,
        CancellationToken cancellationToken = default)
    {
        return progressRepository.GetHistoryAsync(profileId, cancellationToken: cancellationToken);
    }

    public Task MarkLearnedAsync(
        Guid profileId,
        string letter,
        CancellationToken cancellationToken = default)
    {
        return progressRepository.MarkLearnedAsync(profileId, letter, cancellationToken);
    }

    public ExerciseQuestion CreateQuestion(ExerciseType type)
    {
        var correctLetter = AlphabetLetter.All[Random.Shared.Next(AlphabetLetter.All.Count)];
        var distractors = AlphabetLetter.All
            .Where(letter => letter.Uppercase != correctLetter.Uppercase)
            .OrderBy(_ => Random.Shared.Next())
            .Take(3)
            .ToArray();

        var answer = type switch
        {
            ExerciseType.FindLetter => correctLetter.Uppercase,
            ExerciseType.MatchWord => correctLetter.Illustration,
            ExerciseType.FindFirstLetter => correctLetter.Uppercase,
            ExerciseType.MatchCase => correctLetter.Lowercase,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unsupported exercise type.")
        };

        var options = type switch
        {
            ExerciseType.FindLetter => distractors.Select(letter => letter.Uppercase).Append(answer),
            ExerciseType.MatchWord => distractors.Select(letter => letter.Illustration).Append(answer),
            ExerciseType.FindFirstLetter => distractors.Select(letter => letter.Uppercase).Append(answer),
            ExerciseType.MatchCase => distractors.Select(letter => letter.Lowercase).Append(answer),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unsupported exercise type.")
        };

        var prompt = type switch
        {
            ExerciseType.FindLetter => $"Znajdź literę {correctLetter.Uppercase}",
            ExerciseType.MatchWord => $"Wybierz obrazek do słowa „{correctLetter.ExampleWord}”",
            ExerciseType.FindFirstLetter => correctLetter.ExampleWord.StartsWith(
                correctLetter.Lowercase,
                StringComparison.CurrentCultureIgnoreCase)
                    ? $"Na jaką literę zaczyna się słowo „{correctLetter.ExampleWord}”?"
                    : $"Którą literę słyszysz w słowie „{correctLetter.ExampleWord}”?",
            ExerciseType.MatchCase => $"Która mała litera pasuje do {correctLetter.Uppercase}?",
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unsupported exercise type.")
        };

        return new ExerciseQuestion(
            type,
            prompt,
            answer,
            options.OrderBy(_ => Random.Shared.Next()).ToArray(),
            correctLetter.Uppercase);
    }

    public Task RecordAnswerAsync(
        Guid profileId,
        ExerciseQuestion question,
        bool wasCorrect,
        CancellationToken cancellationToken = default)
    {
        return progressRepository.RecordPracticeAsync(
            profileId,
            question.Letter,
            question.Type.ToString(),
            wasCorrect,
            cancellationToken);
    }
}
