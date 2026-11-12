using LearnWithMe.Domain.Entities;

namespace LearnWithMe.Application.Features.Math;

public interface IMathLearningService
{
    IReadOnlyList<MathExerciseOption> ExerciseTypes { get; }

    MathQuestion CreateQuestion(MathTopic topic);

    Task<IReadOnlyList<MathTopicProgress>> GetProgressAsync(
        Guid profileId,
        CancellationToken cancellationToken = default);

    Task RecordAnswerAsync(
        Guid profileId,
        MathTopic topic,
        bool wasCorrect,
        CancellationToken cancellationToken = default);
}
