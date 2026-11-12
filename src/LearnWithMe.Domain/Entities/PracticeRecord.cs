namespace LearnWithMe.Domain.Entities;

public sealed class PracticeRecord
{
    private PracticeRecord()
    {
        Letter = string.Empty;
        ExerciseType = string.Empty;
    }

    public PracticeRecord(Guid profileId, string letter, string exerciseType, bool wasCorrect)
    {
        if (profileId == Guid.Empty)
        {
            throw new ArgumentException("A profile is required.", nameof(profileId));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(letter);
        ArgumentException.ThrowIfNullOrWhiteSpace(exerciseType);
        Id = Guid.NewGuid();
        ProfileId = profileId;
        Letter = letter;
        ExerciseType = exerciseType;
        WasCorrect = wasCorrect;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public Guid Id { get; private set; }

    public Guid ProfileId { get; private set; }

    public string Letter { get; private set; }

    public string ExerciseType { get; private set; }

    public bool WasCorrect { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }
}
