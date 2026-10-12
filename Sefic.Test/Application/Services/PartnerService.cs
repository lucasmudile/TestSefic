using Microsoft.EntityFrameworkCore;
using Sefic.Api.Application.Dtos;
using Sefic.Api.Application.Exceptions;
using Sefic.Api.Application.Interfaces;
using Sefic.Api.Domain;
using Sefic.Api.Domain.Entities;
using Sefic.Api.Domain.Services;
using static Sefic.Api.Application.Services.PartnerFieldValidator;

namespace Sefic.Api.Application.Services;

public sealed class PartnerService(IPartnerRepository repository) : IPartnerService
{
    public async Task<PartnerResponse> CreateAsync(CreatePartnerRequest request, CancellationToken cancellationToken)
    {
        var partner = new Partner
        {
            Id = Guid.NewGuid(),
            Name = ValidateName(request.Name),
            TaxNumber = ValidateTaxNumber(request.TaxNumber),
            Type = ValidateType(request.Type),
            Email = ValidateEmail(request.Email),
            CreditLimit = ValidateCreditLimit(request.CreditLimit),
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await repository.AddAsync(partner, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return PartnerResponse.FromEntity(partner);
    }

    public async Task<PartnerResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var partner = await repository.FindByIdAsync(id, cancellationToken)
            ?? throw new PartnerNotFoundException($"Partner com o id '{id}' não foi encontrado.");

        return PartnerResponse.FromEntity(partner);
    }

    public async Task<PagedResponse<PartnerResponse>> ListAsync(PartnerListQuery query, CancellationToken cancellationToken)
    {
        var page = query.Page ?? 1;
        if (page < 1)
        {
            throw new InvalidRequestException("O parâmetro de query 'page' deve ser maior ou igual a 1.");
        }

        var pageSize = query.PageSize ?? 20;
        if (pageSize is < 1 or > 100)
        {
            throw new InvalidRequestException("O parâmetro de query 'pageSize' deve estar entre 1 e 100.");
        }

        var filtered = repository.Query();

        if (!query.IncludeInactive)
        {
            filtered = filtered.Where(p => p.IsActive);
        }

        if (!string.IsNullOrWhiteSpace(query.Type))
        {
            var type = ValidateType(query.Type);
            List<PartnerType> allowed = type switch
            {
                PartnerType.CUSTOMER => [PartnerType.CUSTOMER, PartnerType.BOTH],
                PartnerType.SUPPLIER => [PartnerType.SUPPLIER, PartnerType.BOTH],
                _ => [PartnerType.BOTH]
            };
            filtered = filtered.Where(p => allowed.Contains(p.Type));
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var nameSearch = query.Search.Trim().ToLowerInvariant();
            var taxSearch = TaxNumberNormalizer.StripSeparators(query.Search);


            if (taxSearch.Length > 0)
            {
                filtered = filtered.Where(p =>
                    p.Name.ToLower().Contains(nameSearch) || p.TaxNumber.Contains(taxSearch));
            }
            else
            {
                filtered = filtered.Where(p => p.Name.ToLower().Contains(nameSearch));
            }
        }

        filtered = filtered
            .OrderBy(p => p.Name)
            .ThenBy(p => p.Id);

        var totalCount = await filtered.CountAsync(cancellationToken);
        var items = await filtered
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResponse<PartnerResponse>(
            [.. items.Select(PartnerResponse.FromEntity)],
            totalCount,
            page,
            pageSize);
    }

    public async Task<PartnerResponse> UpdateAsync(Guid id, UpdatePartnerRequest request, CancellationToken cancellationToken)
    {
        var partner = await repository.FindByIdAsync(id, cancellationToken)
            ?? throw new PartnerNotFoundException($"Partner com o id '{id}' não foi encontrado.");

        if (request.IsActive is null)
        {
            throw new InvalidRequestException("O campo 'isActive' é obrigatório.");
        }

        var name = ValidateName(request.Name);
        var taxNumber = ValidateTaxNumber(request.TaxNumber);
        var type = ValidateType(request.Type);
        var email = ValidateEmail(request.Email);
        var creditLimit = ValidateCreditLimit(request.CreditLimit);
        var clientVersion = ValidateVersion(request.Version);

        repository.SetOriginalVersion(partner, clientVersion);

        partner.Name = name;
        partner.TaxNumber = taxNumber;
        partner.Type = type;
        partner.Email = email;
        partner.CreditLimit = creditLimit;
        partner.IsActive = request.IsActive.Value;
        partner.UpdatedAt = DateTime.UtcNow;

        await repository.SaveChangesAsync(cancellationToken);

        return PartnerResponse.FromEntity(partner);
    }
}
