namespace LearnWithMe.Application.Features.Alphabet;

public sealed record ExerciseQuestion(
    ExerciseType Type,
    string Prompt,
    string CorrectAnswer,
    IReadOnlyList<string> Options,
    string Letter);
