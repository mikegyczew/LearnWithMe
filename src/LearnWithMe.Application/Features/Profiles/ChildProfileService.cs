using LearnWithMe.Application.Abstractions;
using LearnWithMe.Domain.Entities;

namespace LearnWithMe.Application.Features.Profiles;

public sealed class ChildProfileService(IChildProfileRepository profileRepository) : IChildProfileService
{
    public Task<IReadOnlyList<ChildProfile>> GetProfilesAsync(
        CancellationToken cancellationToken = default)
    {
        return profileRepository.GetAllAsync(cancellationToken);
    }

    public async Task<ChildProfile> CreateProfileAsync(
        string name,
        string? avatar = null,
        CancellationToken cancellationToken = default)
    {
        var profile = new ChildProfile(name, avatar);
        await profileRepository.AddAsync(profile, cancellationToken);
        return profile;
    }

    public Task DeleteProfileAsync(Guid profileId, CancellationToken cancellationToken = default)
    {
        if (profileId == Guid.Empty)
        {
            throw new ArgumentException("Profile ID cannot be empty.", nameof(profileId));
        }

        return profileRepository.DeleteAsync(profileId, cancellationToken);
    }
}
