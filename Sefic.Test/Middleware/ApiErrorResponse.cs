namespace Sefic.Api.Middleware;

public sealed record ApiErrorResponse(int Status, string ErrorCode, string Message);
