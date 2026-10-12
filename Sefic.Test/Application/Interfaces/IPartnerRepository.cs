using Sefic.Api.Domain.Entities;

namespace Sefic.Api.Application.Interfaces;

public interface IPartnerRepository
{
    IQueryable<Partner> Query();

    Task<Partner?> FindByIdAsync(Guid id, CancellationToken cancellationToken);

    Task AddAsync(Partner partner, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);

    void SetOriginalVersion(Partner partner, byte[] clientVersion);
}
