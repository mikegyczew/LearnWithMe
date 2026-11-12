using LearnWithMe.Application.Abstractions;
using LearnWithMe.Application.Features.Profiles;
using LearnWithMe.Domain.Entities;

namespace LearnWithMe.Application.Tests;

public class ChildProfileServiceTests
{
    [Fact]
    public async Task CreateProfileAsync_StoresAndReturnsProfile()
    {
        var repository = new InMemoryChildProfileRepository();
        var service = new ChildProfileService(repository);

        var profile = await service.CreateProfileAsync("  Ania ", "🐻");

        Assert.Same(profile, Assert.Single(repository.Profiles));
        Assert.Equal("Ania", profile.Name);
        Assert.Equal("🐻", profile.Avatar);
    }

    [Fact]
    public async Task GetProfilesAsync_ReturnsRepositoryProfiles()
    {
        var expected = new ChildProfile("Ania");
        var repository = new InMemoryChildProfileRepository();
        await repository.AddAsync(expected);
        var service = new ChildProfileService(repository);

        var profiles = await service.GetProfilesAsync();

        Assert.Same(expected, Assert.Single(profiles));
    }

    [Fact]
    public async Task DeleteProfileAsync_DeletesProfileFromRepository()
    {
        var profile = new ChildProfile("Ania");
        var repository = new InMemoryChildProfileRepository();
        await repository.AddAsync(profile);
        var service = new ChildProfileService(repository);

        await service.DeleteProfileAsync(profile.Id);

        Assert.Empty(repository.Profiles);
    }

    private sealed class InMemoryChildProfileRepository : IChildProfileRepository
    {
        public List<ChildProfile> Profiles { get; } = [];

        public Task<IReadOnlyList<ChildProfile>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<ChildProfile>>(Profiles.ToArray());
        }

        public Task AddAsync(ChildProfile profile, CancellationToken cancellationToken = default)
        {
            Profiles.Add(profile);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Guid profileId, CancellationToken cancellationToken = default)
        {
            Profiles.RemoveAll(profile => profile.Id == profileId);
            return Task.CompletedTask;
        }
    }
}
