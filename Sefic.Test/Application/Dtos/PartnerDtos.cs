using Sefic.Api.Domain.Entities;

namespace Sefic.Api.Application.Dtos;

public record PartnerResponse(
    Guid Id,
    string Name,
    string TaxNumber,
    string Type,
    string? Email,
    decimal CreditLimit,
    bool IsActive,
    string Version,
    DateTime CreatedAt,
    DateTime UpdatedAt)
{
    public static PartnerResponse FromEntity(Partner partner) => new(
        partner.Id,
        partner.Name,
        partner.TaxNumber,
        partner.Type.ToString(),
        partner.Email,
        partner.CreditLimit,
        partner.IsActive,
        Convert.ToBase64String(partner.Version),
        DateTime.SpecifyKind(partner.CreatedAt, DateTimeKind.Utc),
        DateTime.SpecifyKind(partner.UpdatedAt, DateTimeKind.Utc));
}

public record PagedResponse<T>(IReadOnlyList<T> Items, long TotalCount, int Page, int PageSize);

public class PartnerListQuery
{
    public string? Search { get; set; }

    public string? Type { get; set; }

    public bool IncludeInactive { get; set; }

    public int? Page { get; set; }

    public int? PageSize { get; set; }
}
