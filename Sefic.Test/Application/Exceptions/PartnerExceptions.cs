namespace Sefic.Api.Application.Exceptions;

public abstract class PartnerException(int status, string errorCode, string message) : Exception(message)
{
    public int Status { get; } = status;

    public string ErrorCode { get; } = errorCode;
}

public sealed class InvalidRequestException(string message)
    : PartnerException(400, "INVALID_REQUEST", message);

public sealed class PartnerNotFoundException(string message)
    : PartnerException(404, "PARTNER_NOT_FOUND", message);

public sealed class DuplicateTaxNumberException(string message)
    : PartnerException(409, "DUPLICATE_TAX_NUMBER", message);

public sealed class VersionConflictException(string message)
    : PartnerException(409, "VERSION_CONFLICT", message);
