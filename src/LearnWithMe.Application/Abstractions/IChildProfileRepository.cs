using LearnWithMe.Domain.Entities;

namespace LearnWithMe.Application.Abstractions;

public interface IChildProfileRepository
{
    Task<IReadOnlyList<ChildProfile>> GetAllAsync(CancellationToken cancellationToken = default);

    Task AddAsync(ChildProfile profile, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid profileId, CancellationToken cancellationToken = default);
}
