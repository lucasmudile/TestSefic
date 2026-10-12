using System.Net;
using System.Text;
using Xunit.Sdk;

namespace Sefic.Api.Tests;

[Collection("Api")]
public abstract class ApiTestBase(ApiFixture fixture)
{
    private static int _nifSequence;

    protected HttpClient Client { get; } = fixture.Client;

    protected static string NewNif() =>
        (800_000_000 + Interlocked.Increment(ref _nifSequence)).ToString();

    protected Task<HttpResponseMessage> PostCreateJson(object body) =>
        Client.PostAsync("/api/partners", ApiJson.Json(body));

    protected Task<HttpResponseMessage> PostCreateRawJson(string json) =>
        Client.PostAsync("/api/partners", new StringContent(json, Encoding.UTF8, "application/json"));

    protected Task<HttpResponseMessage> PutAsync(Guid id, object body) =>
        Client.PutAsync($"/api/partners/{id}", ApiJson.Json(body));

    protected Task<HttpResponseMessage> GetAsync(string uri) => Client.GetAsync(uri);

    protected async Task<PartnerDto> CreateAsync(string name, string taxNumber, string type,
        string? email = null, decimal creditLimit = 1.5m)
    {
        var response = await PostCreateJson(new { name, taxNumber, type, email, creditLimit });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return await response.ReadAsync<PartnerDto>();
    }

    protected async Task<PartnerDto> GetPartnerAsync(Guid id)
    {
        var response = await Client.GetAsync($"/api/partners/{id}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return await response.ReadAsync<PartnerDto>();
    }

    protected async Task<PagedDto> GetListAsync(string queryString)
    {
        var response = await Client.GetAsync($"/api/partners?{queryString}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return await response.ReadAsync<PagedDto>();
    }

    protected async Task<PartnerDto> UpdateAndAssertOkAsync(PartnerDto current, string? name = null,
        string? taxNumber = null, string? type = null, decimal? creditLimit = null, bool? isActive = null,
        string? version = null)
    {
        var response = await PutAsync(current.Id, BuildUpdate(current, name, taxNumber, type, creditLimit, isActive, version));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return await response.ReadAsync<PartnerDto>();
    }

    protected static object BuildUpdate(PartnerDto current, string? name = null, string? taxNumber = null,
        string? type = null, decimal? creditLimit = null, bool? isActive = null, string? version = null,
        string? email = null, bool emailUnchanged = true) =>
        new
        {
            name = name ?? current.Name,
            taxNumber = taxNumber ?? current.TaxNumber,
            type = type ?? current.Type,
            email = emailUnchanged ? current.Email : email,
            creditLimit = creditLimit ?? current.CreditLimit,
            isActive = isActive ?? current.IsActive,
            version = version ?? current.Version
        };

    protected async Task<ErrorDto> ReadErrorAsync(HttpResponseMessage response)
    {
        var error = await response.ReadAsync<ErrorDto>();
        Assert.Equal(error.Status, (int)response.StatusCode);
        return error;
    }

    protected static void AssertNoFailure(params HttpResponseMessage[] responses)
    {
        foreach (var response in responses)
        {
            if ((int)response.StatusCode >= 500)
            {
                throw new XunitException($"Unexpected server error: {(int)response.StatusCode} {response.RequestMessage?.RequestUri}");
            }
        }
    }
}
