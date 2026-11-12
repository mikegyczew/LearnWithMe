namespace LearnWithMe.Domain.Entities;

public sealed class MathTopicProgress
{
    private MathTopicProgress()
    {
    }

    public MathTopicProgress(Guid profileId, MathTopic topic)
    {
        if (profileId == Guid.Empty)
        {
            throw new ArgumentException("A profile is required.", nameof(profileId));
        }

        if (!Enum.IsDefined(topic))
        {
            throw new ArgumentOutOfRangeException(nameof(topic));
        }

        ProfileId = profileId;
        Topic = topic;
    }

    public Guid ProfileId { get; private set; }

    public MathTopic Topic { get; private set; }

    public int Attempts { get; private set; }

    public int Correct { get; private set; }

    public void RecordAnswer(bool wasCorrect)
    {
        Attempts++;
        if (wasCorrect)
        {
            Correct++;
        }
    }
}
