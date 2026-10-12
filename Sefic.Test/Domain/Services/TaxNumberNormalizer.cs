namespace Sefic.Api.Domain.Services;

public static class TaxNumberNormalizer
{
    public static string? Normalize(string? value)
    {
        if (value is null)
        {
            return null;
        }

        var cleaned = new string([.. value.Where(c => c is not (' ' or '.' or '-' ))]);

        if (cleaned.Length != 9)
        {
            return null;
        }

        foreach (var c in cleaned)
        {
            if (c is < '0' or > '9')
            {
                return null;
            }
        }

        return cleaned;
    }

    public static string StripSeparators(string value) =>
        new([.. value.Where(c => c is not (' ' or '.' or '-'))]);
}
