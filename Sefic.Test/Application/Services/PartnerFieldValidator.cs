using Sefic.Api.Application.Exceptions;
using Sefic.Api.Domain;
using Sefic.Api.Domain.Services;
using System.Text.RegularExpressions;

namespace Sefic.Api.Application.Services;

public static partial class PartnerFieldValidator
{
    public static string ValidateName(string? raw)
    {
        if (raw is null)
        {
            throw new InvalidRequestException("O campo 'name' é obrigatório.");
        }

        var name = raw.Trim();
        if (name.Length is < 2 or > 100)
        {
            throw new InvalidRequestException("O campo 'name' deve ter entre 2 e 100 caracteres depois de Trim.");
        }

        return name;
    }

    public static string ValidateTaxNumber(string? raw)
    {
        if (raw is null)
        {
            throw new InvalidRequestException("O campo 'taxNumber' é obrigatório.");
        }

        var normalized = TaxNumberNormalizer.Normalize(raw);
        if (normalized is null)
        {
            throw new InvalidRequestException("O campo 'taxNumber' deve conter exatamente 9 dígitos ASCII depois de remover espaços, pontos e hífenes.");
        }

        return normalized;
    }

    public static PartnerType ValidateType(string? raw)
    {
        if (raw is null)
        {
            throw new InvalidRequestException("O campo 'type' é obrigatório.");
        }

        if (!Enum.TryParse<PartnerType>(raw, ignoreCase: true, out var type))
        {
            throw new InvalidRequestException($"O campo 'type' deve ser CUSTOMER, SUPPLIER ou BOTH. Recebido: '{raw}'.");
        }

        return type;
    }

    public static string? ValidateEmail(string? raw)
    {
        if (raw is null)
        {
            return null;
        }

        var email = raw.Trim();
        if (email.Length == 0)
        {
            return null;
        }

        if (email.Length > 254 || !EmailRegex().IsMatch(email))
        {
            throw new InvalidRequestException("O campo 'email' tem um formato inválido.");
        }

        return email;
    }

    public static decimal ValidateCreditLimit(decimal? value)
    {
        if (value is null)
        {
            throw new InvalidRequestException("O campo 'creditLimit' é obrigatório.");
        }

        var amount = value.Value;
        var scale = (decimal.GetBits(amount)[3] >> 16) & 0x7F;
        if (scale > 2)
        {
            throw new InvalidRequestException("O campo 'creditLimit' deve ter no máximo 2 casas decimais. Não é aplicado arredondamento.");
        }

        if (amount < 0m || amount > 1_000_000m)
        {
            throw new InvalidRequestException("O campo 'creditLimit' deve estar entre 0 e 1000000.");
        }

        return amount;
    }

    public static byte[] ValidateVersion(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            throw new InvalidRequestException("O campo 'version' é obrigatório.");
        }

        try
        {
            return Convert.FromBase64String(raw);
        }
        catch (FormatException)
        {
            throw new InvalidRequestException("O campo 'version' deve ser uma string Base64 devolvida anteriormente pela API.");
        }
    }

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.CultureInvariant)]
    private static partial Regex EmailRegex();
}
