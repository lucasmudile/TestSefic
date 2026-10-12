namespace Sefic.Api.Domain.Entities;

public class Partner
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string TaxNumber { get; set; } = string.Empty;

    public PartnerType Type { get; set; }

    public string? Email { get; set; }

    public decimal CreditLimit { get; set; }

    public bool IsActive { get; set; } = true;

    public byte[] Version { get; set; } = [];

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
