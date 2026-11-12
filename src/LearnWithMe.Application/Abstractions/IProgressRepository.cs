using LearnWithMe.Domain.Entities;

namespace LearnWithMe.Application.Abstractions;

public interface IProgressRepository
{
    Task<IReadOnlyList<LetterProgress>> GetProgressAsync(
        Guid profileId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PracticeRecord>> GetHistoryAsync(
        Guid profileId,
        int limit = 20,
        CancellationToken cancellationToken = default);

    Task MarkLearnedAsync(
        Guid profileId,
        string letter,
        CancellationToken cancellationToken = default);

    Task RecordPracticeAsync(
        Guid profileId,
        string letter,
        string exerciseType,
        bool wasCorrect,
        CancellationToken cancellationToken = default);
}
