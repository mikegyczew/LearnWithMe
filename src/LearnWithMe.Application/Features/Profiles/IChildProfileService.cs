using LearnWithMe.Domain.Entities;

namespace LearnWithMe.Application.Features.Profiles;

public interface IChildProfileService
{
    Task<IReadOnlyList<ChildProfile>> GetProfilesAsync(CancellationToken cancellationToken = default);

    Task<ChildProfile> CreateProfileAsync(
        string name,
        string? avatar = null,
        CancellationToken cancellationToken = default);

    Task DeleteProfileAsync(Guid profileId, CancellationToken cancellationToken = default);
}
