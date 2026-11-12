using LearnWithMe.Domain.Entities;

namespace LearnWithMe.Application.Abstractions;

public interface IMathProgressRepository
{
    Task<IReadOnlyList<MathTopicProgress>> GetProgressAsync(
        Guid profileId,
        CancellationToken cancellationToken = default);

    Task RecordAnswerAsync(
        Guid profileId,
        MathTopic topic,
        bool wasCorrect,
        CancellationToken cancellationToken = default);
}
