using LearnWithMe.Domain.Entities;

namespace LearnWithMe.Application.Features.Math;

public sealed record MathQuestion(
    MathTopic Topic,
    string Prompt,
    string VisualHint,
    int CorrectAnswer,
    IReadOnlyList<int> Options,
    string NumberName,
    IReadOnlyList<NumberReadingExample> ReadingExamples);
