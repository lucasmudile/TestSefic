namespace Sefic.Api.Application.Dtos;

public record CreatePartnerRequest(
    string? Name,
    string? TaxNumber,
    string? Type,
    string? Email,
    decimal? CreditLimit);

public record UpdatePartnerRequest(
    string? Name,
    string? TaxNumber,
    string? Type,
    string? Email,
    decimal? CreditLimit,
    bool? IsActive,
    string? Version);
