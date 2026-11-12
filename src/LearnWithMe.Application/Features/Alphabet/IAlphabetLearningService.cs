using LearnWithMe.Domain.Entities;

namespace LearnWithMe.Application.Features.Alphabet;

public interface IAlphabetLearningService
{
    Task<IReadOnlyList<LetterProgress>> GetProgressAsync(
        Guid profileId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PracticeRecord>> GetHistoryAsync(
        Guid profileId,
        CancellationToken cancellationToken = default);

    Task MarkLearnedAsync(
        Guid profileId,
        string letter,
        CancellationToken cancellationToken = default);

    ExerciseQuestion CreateQuestion(ExerciseType type);

    Task RecordAnswerAsync(
        Guid profileId,
        ExerciseQuestion question,
        bool wasCorrect,
        CancellationToken cancellationToken = default);
}
