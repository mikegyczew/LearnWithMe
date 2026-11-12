using LearnWithMe.Domain.Entities;

namespace LearnWithMe.Domain.Tests;

public class ChildProfileTests
{
    [Fact]
    public void Constructor_TrimsNameAndStoresOptionalAvatar()
    {
        var profile = new ChildProfile("  Ania  ", "🐻");

        Assert.NotEqual(Guid.Empty, profile.Id);
        Assert.Equal("Ania", profile.Name);
        Assert.Equal("🐻", profile.Avatar);
        Assert.Equal(DateTimeKind.Utc, profile.CreatedAtUtc.Kind);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("1234567890123456789012345678901")]
    public void Constructor_RejectsInvalidName(string name)
    {
        Assert.Throws<ArgumentException>(() => new ChildProfile(name));
    }

    [Fact]
    public void Constructor_RejectsAvatarLongerThanMaximum()
    {
        Assert.Throws<ArgumentException>(() => new ChildProfile("Ania", new string('a', 33)));
    }

    [Fact]
    public void PracticeProgress_IsTrackedPerLetterAndProfile()
    {
        var progress = new LetterProgress(Guid.NewGuid(), "Ą");

        progress.RecordPractice(wasCorrect: false);
        Assert.Equal(1, progress.PracticeCount);
        Assert.Equal(0, progress.CorrectCount);
        Assert.False(progress.IsLearned);

        progress.RecordPractice(wasCorrect: true);
        Assert.Equal(2, progress.PracticeCount);
        Assert.Equal(1, progress.CorrectCount);
        Assert.True(progress.IsLearned);
    }
}
