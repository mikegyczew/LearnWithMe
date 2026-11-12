namespace LearnWithMe.Domain.Entities;

public sealed class LetterProgress
{
    private LetterProgress()
    {
        Letter = string.Empty;
    }

    public LetterProgress(Guid profileId, string letter)
    {
        if (profileId == Guid.Empty)
        {
            throw new ArgumentException("A profile is required.", nameof(profileId));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(letter);
        ProfileId = profileId;
        Letter = letter;
        LastPracticedAtUtc = DateTime.UtcNow;
    }

    public Guid ProfileId { get; private set; }

    public string Letter { get; private set; }

    public int PracticeCount { get; private set; }

    public int CorrectCount { get; private set; }

    public DateTime LastPracticedAtUtc { get; private set; }

    public bool IsLearned { get; private set; }

    public void MarkLearned()
    {
        IsLearned = true;
        LastPracticedAtUtc = DateTime.UtcNow;
    }

    public void RecordPractice(bool wasCorrect)
    {
        PracticeCount++;
        if (wasCorrect)
        {
            CorrectCount++;
            IsLearned = true;
        }

        LastPracticedAtUtc = DateTime.UtcNow;
    }
}
