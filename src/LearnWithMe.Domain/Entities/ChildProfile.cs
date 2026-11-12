namespace LearnWithMe.Domain.Entities;

public sealed class ChildProfile
{
    private const int MaximumNameLength = 30;
    private const int MaximumAvatarLength = 32;

    private ChildProfile()
    {
        Name = string.Empty;
    }

    public ChildProfile(string name, string? avatar = null)
    {
        ArgumentNullException.ThrowIfNull(name);

        var normalizedName = name.Trim();
        if (normalizedName.Length is 0 or > MaximumNameLength)
        {
            throw new ArgumentException(
                $"A profile name must contain between 1 and {MaximumNameLength} characters.",
                nameof(name));
        }

        if (avatar?.Length > MaximumAvatarLength)
        {
            throw new ArgumentException(
                $"An avatar must contain no more than {MaximumAvatarLength} characters.",
                nameof(avatar));
        }

        Id = Guid.NewGuid();
        Name = normalizedName;
        Avatar = string.IsNullOrWhiteSpace(avatar) ? null : avatar.Trim();
        CreatedAtUtc = DateTime.UtcNow;
    }

    public Guid Id { get; private set; }

    public string Name { get; private set; }

    public string? Avatar { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }
}
