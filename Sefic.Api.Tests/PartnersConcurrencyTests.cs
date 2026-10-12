using System.Net;

namespace Sefic.Api.Tests;

public class PartnersConcurrencyTests(ApiFixture fixture) : ApiTestBase(fixture)
{
    // Cenário 5 do enunciado: criação concorrente do mesmo NIF
    [Fact]
    public async Task DoisPostsSimultaneos_MesmoNif_UmDevolve201Outro409_ExisteUmUnicoRegisto()
    {
        var nif = NewNif();
        var formatted = $"{nif[..3]}.{nif[3..6]}.{nif[6..]}";

        Task<HttpResponseMessage> Post() => PostCreateJson(new
        {
            name = "Mu Corrida",
            taxNumber = formatted,
            type = "CUSTOMER",
            email = (string?)null,
            creditLimit = 3.25
        });

        var responses = await Task.WhenAll(Post(), Post());

        AssertNoFailure(responses);
        var codes = responses.Select(r => r.StatusCode).OrderBy(c => (int)c).ToArray();
        Assert.Equal([HttpStatusCode.Created, HttpStatusCode.Conflict], codes);

        var list = await GetListAsync($"search={nif}&includeInactive=true&pageSize=100");
        Assert.Equal(1L, list.TotalCount);
    }

    // Cenário 5 do enunciado: atualização concorrente com a mesma Version
    [Fact]
    public async Task DoisPutsSimultaneos_MesmaVersion_UmDevolve200Outro409()
    {
        var created = await CreateAsync("Nu Corrida", NewNif(), "CUSTOMER");
        var originalVersion = created.Version;

        var responses = await Task.WhenAll(
            PutAsync(created.Id, BuildUpdate(created, name: "Nu Corrida A", version: originalVersion)),
            PutAsync(created.Id, BuildUpdate(created, name: "Nu Corrida B", version: originalVersion)));

        AssertNoFailure(responses);
        var codes = responses.Select(r => r.StatusCode).OrderBy(c => (int)c).ToArray();
        Assert.Equal([HttpStatusCode.OK, HttpStatusCode.Conflict], codes);

        var final = await GetPartnerAsync(created.Id);
        Assert.True(final.Name is "Nu Corrida A" or "Nu Corrida B");
        Assert.NotEqual(originalVersion, final.Version);

        var reuse = await PutAsync(created.Id, BuildUpdate(final, name: "Nu Corrida C", version: originalVersion));
        Assert.Equal(HttpStatusCode.Conflict, reuse.StatusCode);
        Assert.Equal("VERSION_CONFLICT", (await ReadErrorAsync(reuse)).ErrorCode);
    }
}
