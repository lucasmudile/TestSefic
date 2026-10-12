using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Sefic.Api.Application.Exceptions;
using Sefic.Api.Application.Interfaces;
using Sefic.Api.Domain.Entities;

namespace Sefic.Api.Infrastructure.Persistence;

public class EfPartnerRepository(SeficDbContext context) : IPartnerRepository
{
    public IQueryable<Partner> Query() => context.Partners.AsNoTracking();

    public Task<Partner?> FindByIdAsync(Guid id, CancellationToken cancellationToken) =>
        context.Partners.FindAsync([id], cancellationToken).AsTask();

    public Task AddAsync(Partner partner, CancellationToken cancellationToken) =>
        context.Partners.AddAsync(partner, cancellationToken).AsTask();

    public void SetOriginalVersion(Partner partner, byte[] clientVersion) =>
        context.Entry(partner).Property(nameof(Partner.Version)).OriginalValue = clientVersion;

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new VersionConflictException(
                "A version fornecida está desatualizada. Releia o partner e tente novamente com a version atual.");
        }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex))
        {
            var taxNumber = ex.Entries
                .Select(entry => entry.Entity as Partner)
                .FirstOrDefault(partner => partner is not null)?.TaxNumber;

            throw new DuplicateTaxNumberException(
                $"O TaxNumber '{taxNumber}' já está registado. Só é permitido um registo por NIF.");
        }
    }

    private static bool IsUniqueViolation(DbUpdateException ex) =>
        ex.InnerException is SqlException sql && (sql.Number is 2601 or 2627);
}
