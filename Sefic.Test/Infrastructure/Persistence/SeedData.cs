using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sefic.Api.Domain;
using Sefic.Api.Domain.Entities;

namespace Sefic.Api.Infrastructure.Persistence;

public static class SeedData
{
    private static readonly DateTime BaseDate = new(2026, 1, 15, 12, 0, 0, DateTimeKind.Utc);

    public static void Apply(EntityTypeBuilder<Partner> builder)
    {
        builder.HasData(
            new Partner
            {
                Id = Guid.Parse("7f3a9c2e-6b41-4d85-a027-e9c6b51f834d"),
                Name = "Maria Santos",
                TaxNumber = "009876543",
                Type = PartnerType.CUSTOMER,
                Email = "maria.santos@gmail.com",
                CreditLimit = 1500.50m,
                IsActive = true,
                CreatedAt = BaseDate,
                UpdatedAt = BaseDate
            },
            new Partner
            {
                Id = Guid.Parse("2f3a9c2e-6b41-4d85-a027-e9c6b51f834d"),
                Name = "Lucas Santana",
                TaxNumber = "002345678",
                Type = PartnerType.SUPPLIER,
                Email = "lucasmudile@gmail.com",
                CreditLimit = 20000m,
                IsActive = true,
                CreatedAt = BaseDate,
                UpdatedAt = BaseDate
            },
            new Partner
            {
                Id = Guid.Parse("7f369c2e-6b41-4d85-a027-e9c6b51f834d"),
                Name = "Maria Gomes",
                TaxNumber = "003456789",
                Type = PartnerType.BOTH,
                Email = null,
                CreditLimit = 5000m,
                IsActive = true,
                CreatedAt = BaseDate,
                UpdatedAt = BaseDate
            },
            new Partner
            {
                Id = Guid.Parse("7f3a9c2e-6b41-4d85-a027-e9c6b51f684d"),
                Name = "Carlos Nunes",
                TaxNumber = "004567890",
                Type = PartnerType.CUSTOMER,
                Email = null,
                CreditLimit = 0m,
                IsActive = false,
                CreatedAt = BaseDate,
                UpdatedAt = BaseDate
            },
            new Partner
            {
                Id = Guid.Parse("7f3a9c2e-6871-4d85-a027-e9c6b51f834d"),
                Name = "Beta Logistica",
                TaxNumber = "005678901",
                Type = PartnerType.CUSTOMER,
                Email = "contacto@gmail.com",
                CreditLimit = 750m,
                IsActive = true,
                CreatedAt = BaseDate,
                UpdatedAt = BaseDate
            },
            new Partner
            {
                Id = Guid.Parse("7f387c2e-6b41-4d85-a027-e9c6b51f834d"),
                Name = "Beta Logistica",
                TaxNumber = "006789012",
                Type = PartnerType.SUPPLIER,
                Email = null,
                CreditLimit = 1000000m,
                IsActive = true,
                CreatedAt = BaseDate,
                UpdatedAt = BaseDate
            });
    }
}
