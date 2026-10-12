using Sefic.Api.Application.Dtos;

namespace Sefic.Api.Application.Interfaces;

public interface IPartnerService
{
    Task<PartnerResponse> CreateAsync(CreatePartnerRequest request, CancellationToken cancellationToken);

    Task<PartnerResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<PagedResponse<PartnerResponse>> ListAsync(PartnerListQuery query, CancellationToken cancellationToken);

    Task<PartnerResponse> UpdateAsync(Guid id, UpdatePartnerRequest request, CancellationToken cancellationToken);
}
