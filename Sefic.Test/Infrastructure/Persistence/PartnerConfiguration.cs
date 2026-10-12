using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sefic.Api.Domain.Entities;

namespace Sefic.Api.Infrastructure.Persistence;

public class PartnerConfiguration : IEntityTypeConfiguration<Partner>
{
    public void Configure(EntityTypeBuilder<Partner> builder)
    {
        builder.ToTable("Partners", table =>
            table.HasCheckConstraint("CK_Partners_Type", "[Type] IN ('CUSTOMER', 'SUPPLIER', 'BOTH')"));

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedNever();

        builder.Property(p => p.Name).HasMaxLength(100).IsRequired();

        builder.Property(p => p.TaxNumber).HasMaxLength(9).IsFixedLength().IsRequired();
        builder.HasIndex(p => p.TaxNumber).IsUnique();

        builder.Property(p => p.Type).HasConversion<string>().HasMaxLength(10);

        builder.Property(p => p.Email).HasMaxLength(254);

        builder.Property(p => p.CreditLimit).HasPrecision(18, 2);

        builder.Property(p => p.IsActive);

        builder.Property(p => p.Version).IsRowVersion();

        builder.Property(p => p.CreatedAt).IsRequired();
        builder.Property(p => p.UpdatedAt).IsRequired();

        SeedData.Apply(builder);
    }
}
