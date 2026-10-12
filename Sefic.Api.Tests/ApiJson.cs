using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace Sefic.Api.Tests;

public record PartnerDto(
    Guid Id,
    string Name,
    string TaxNumber,
    string Type,
    string? Email,
    decimal CreditLimit,
    bool IsActive,
    string Version,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public record PagedDto(
    List<PartnerDto> Items,
    long TotalCount,
    int Page,
    int PageSize);

public record ErrorDto(int Status, string ErrorCode, string Message);

public static class ApiJson
{
    public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);

    public static StringContent Json(object payload) =>
        new(JsonSerializer.Serialize(payload, Options), Encoding.UTF8, "application/json");

    public static async Task<T> ReadAsync<T>(this HttpResponseMessage response) =>
        (await response.Content.ReadFromJsonAsync<T>(Options))!;
}
