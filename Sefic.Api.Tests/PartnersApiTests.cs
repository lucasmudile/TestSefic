using System.Net;

namespace Sefic.Api.Tests;

public class PartnersApiTests(ApiFixture fixture) : ApiTestBase(fixture)
{
    // Cenário 1 do enunciado
    [Fact]
    public async Task Criar_ComEspacosEPontos_Normaliza_E_ReservaNifMesmoAposDesativacao()
    {
        var response = await PostCreateJson(new
        {
            name = " Ana Silva ",
            taxNumber = "001.234.567",
            type = "CUSTOMER",
            email = " ana@example.com ",
            creditLimit = 250.75
        });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var created = await response.ReadAsync<PartnerDto>();
        Assert.Equal("Ana Silva", created.Name);
        Assert.Equal("001234567", created.TaxNumber);
        Assert.Equal("ana@example.com", created.Email);
        Assert.True(created.IsActive);
        Assert.NotEmpty(Convert.FromBase64String(created.Version));
        Assert.NotNull(response.Headers.Location);
        Assert.EndsWith($"/api/partners/{created.Id}", response.Headers.Location!.ToString());

        var duplicate = await PostCreateJson(new
        {
            name = "Outro Nome",
            taxNumber = "001 234 567",
            type = "SUPPLIER",
            email = (string?)null,
            creditLimit = 0
        });
        Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);
        Assert.Equal("DUPLICATE_TAX_NUMBER", (await ReadErrorAsync(duplicate)).ErrorCode);

        var deactivated = await UpdateAndAssertOkAsync(created, isActive: false);
        Assert.False(deactivated.IsActive);

        var stillReserved = await PostCreateJson(new
        {
            name = "Terceiro Nome",
            taxNumber = "001.234.567",
            type = "CUSTOMER",
            email = (string?)null,
            creditLimit = 10
        });
        Assert.Equal(HttpStatusCode.Conflict, stillReserved.StatusCode);
    }

    // Cenário 2 do enunciado
    [Fact]
    public async Task FiltroPorType_BothApareceEmTodos_CustomerNaoApareceEmSupplier()
    {
        await CreateAsync("Iota Duplo Both", NewNif(), "BOTH");
        await CreateAsync("Iota Duplo Cust", NewNif(), "CUSTOMER");

        var customerFilter = await GetListAsync("search=Iota%20Duplo&type=CUSTOMER&pageSize=100");
        Assert.Equal(["Iota Duplo Both", "Iota Duplo Cust"], customerFilter.Items.Select(i => i.Name).Order().ToArray());

        var supplierFilter = await GetListAsync("search=Iota%20Duplo&type=SUPPLIER&pageSize=100");
        Assert.Equal(["Iota Duplo Both"], supplierFilter.Items.Select(i => i.Name).ToArray());

        var bothFilter = await GetListAsync("search=Iota%20Duplo&type=BOTH&pageSize=100");
        Assert.Equal(["Iota Duplo Both"], bothFilter.Items.Select(i => i.Name).ToArray());
    }

    // Cenário 3 do enunciado
    [Fact]
    public async Task DesativarReativar_SaiDaListagemPorDefecto_MantemSeAcessivelPorId()
    {
        var created = await CreateAsync("Kapa Fluxo", NewNif(), "CUSTOMER");

        Assert.Contains((await GetListAsync("search=Kapa&pageSize=100")).Items, i => i.Id == created.Id);

        await UpdateAndAssertOkAsync(created, isActive: false);

        Assert.DoesNotContain((await GetListAsync("search=Kapa&pageSize=100")).Items, i => i.Id == created.Id);
        Assert.Contains((await GetListAsync("search=Kapa&includeInactive=true&pageSize=100")).Items, i => i.Id == created.Id);

        var byId = await GetPartnerAsync(created.Id);
        Assert.False(byId.IsActive);

        var reactivated = await UpdateAndAssertOkAsync(byId, isActive: true);
        Assert.Contains((await GetListAsync("search=Kapa&pageSize=100")).Items, i => i.Id == reactivated.Id);
    }

    // Cenário 4 do enunciado
    [Fact]
    public async Task Put_MantemProprioNif_MudarParaNifAlheioDa409SemAlterarRegisto()
    {
        var nifA = NewNif();
        var nifB = NewNif();
        var a = await CreateAsync("Lambda Par A", nifA, "CUSTOMER");
        await CreateAsync("Lambda Par B", nifB, "SUPPLIER");

        var renamed = await UpdateAndAssertOkAsync(a, name: "Lambda Par A2");
        Assert.Equal(nifA, renamed.TaxNumber);
        Assert.Equal(a.CreatedAt, renamed.CreatedAt);

        var hijack = await PutAsync(a.Id, BuildUpdate(renamed, taxNumber: nifB));
        Assert.Equal(HttpStatusCode.Conflict, hijack.StatusCode);
        Assert.Equal("DUPLICATE_TAX_NUMBER", (await ReadErrorAsync(hijack)).ErrorCode);

        var unchanged = await GetPartnerAsync(a.Id);
        Assert.Equal("Lambda Par A2", unchanged.Name);
        Assert.Equal(nifA, unchanged.TaxNumber);
    }

    // Cenário 6 do enunciado
    [Fact]
    public async Task PedidosInvalidos_Devolvem400_SemAlterarBaseDeDados()
    {
        var nifOk = NewNif();
        var existing = await CreateAsync("Xi Valido", nifOk, "CUSTOMER");
        var baseline = await GetListAsync("pageSize=100&includeInactive=true");

        var precision = await PostCreateRawJson(
            $$"""{"name":"Xi Precisao","taxNumber":"{{NewNif()}}","type":"CUSTOMER","email":null,"creditLimit":10.999}""");
        Assert.Equal(HttpStatusCode.BadRequest, precision.StatusCode);

        var lettersNif = await PostCreateJson(new { name = "Xi Letras", taxNumber = "12A456789", type = "CUSTOMER", email = (string?)null, creditLimit = 1 });
        Assert.Equal(HttpStatusCode.BadRequest, lettersNif.StatusCode);

        var searchRejectedNif = await GetListAsync("search=12A456789");
        Assert.Equal(0L, searchRejectedNif.TotalCount);

        var shortName = await PostCreateJson(new { name = " A", taxNumber = NewNif(), type = "CUSTOMER", email = (string?)null, creditLimit = 1 });
        Assert.Equal(HttpStatusCode.BadRequest, shortName.StatusCode);

        var missingField = await PostCreateRawJson("""{"taxNumber":"999000111","type":"CUSTOMER","email":null,"creditLimit":1}""");
        Assert.Equal(HttpStatusCode.BadRequest, missingField.StatusCode);

        Assert.Equal(HttpStatusCode.BadRequest, (await GetAsync("/api/partners?page=0")).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await GetAsync("/api/partners?pageSize=0")).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await GetAsync("/api/partners?pageSize=101")).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await GetAsync("/api/partners?type=PARTNER")).StatusCode);

        var malformedVersion = await PutAsync(existing.Id, BuildUpdate(existing, name: "Xi Renomeado", version: "###nao-e-base64###"));
        Assert.Equal(HttpStatusCode.BadRequest, malformedVersion.StatusCode);

        var missingIsActive = await PutAsync(existing.Id, new
        {
            name = "Xi Renomeado",
            taxNumber = nifOk,
            type = "CUSTOMER",
            email = (string?)null,
            creditLimit = 1,
            isActive = (bool?)null,
            version = existing.Version
        });
        Assert.Equal(HttpStatusCode.BadRequest, missingIsActive.StatusCode);

        var after = await GetListAsync("pageSize=100&includeInactive=true");
        Assert.Equal(baseline.TotalCount, after.TotalCount);

        var stillIntact = await GetPartnerAsync(existing.Id);
        Assert.Equal("Xi Valido", stillIntact.Name);
        Assert.Equal(existing.Version, stillIntact.Version);
    }

    // Cenário 7 do enunciado
    [Fact]
    public async Task Listagem_FiltrosCombinados_TotalCountAntesDaPaginacao_DesempatePorId_SearchNaoCasaViaNif()
    {
        var z1 = await CreateAsync("Zeta Iguais", NewNif(), "CUSTOMER");
        var z2 = await CreateAsync("Zeta Iguais", NewNif(), "SUPPLIER");
        var z3 = await CreateAsync("Zeta Iguais", NewNif(), "CUSTOMER");
        await CreateAsync("Zeta Singular", NewNif(), "SUPPLIER");

        var tieBreak = await GetListAsync("search=Zeta%20Iguais&pageSize=100");
        Assert.Equal(3, tieBreak.Items.Count);
        Assert.All(tieBreak.Items, item => Assert.Equal("Zeta Iguais", item.Name));
        Assert.Equal(new[] { z1.Id, z2.Id, z3.Id }.Order().ToArray(), tieBreak.Items.Select(i => i.Id).Order().ToArray());

        // Empate por Id tem de ser determinístico entre pedidos repetidos
        var tieBreakRepeat = await GetListAsync("search=Zeta%20Iguais&pageSize=100");
        Assert.Equal(tieBreak.Items.Select(i => i.Id).ToArray(), tieBreakRepeat.Items.Select(i => i.Id).ToArray());

        var combined = await GetListAsync("search=Zeta&type=CUSTOMER&pageSize=100");
        Assert.Equal(2L, combined.TotalCount);
        Assert.All(combined.Items, item => Assert.Equal("CUSTOMER", item.Type));

        var supplierOnly = await GetListAsync("search=Zeta&type=SUPPLIER&pageSize=100");
        Assert.Contains("Zeta Singular", supplierOnly.Items.Select(i => i.Name));

        var firstPage = await GetListAsync("search=Zeta&page=1&pageSize=3");
        Assert.Equal(4L, firstPage.TotalCount);
        Assert.Equal(3, firstPage.Items.Count);

        var beyond = await GetListAsync("search=Zeta&page=99&pageSize=20");
        Assert.Empty(beyond.Items);
        Assert.Equal(4L, beyond.TotalCount);

        var ana = await CreateAsync("Omega Ana Nome", NewNif(), "CUSTOMER");
        var anaSearch = await GetListAsync("search=ana&pageSize=100");
        Assert.Contains(anaSearch.Items, i => i.Id == ana.Id);
        Assert.All(anaSearch.Items, item => Assert.Contains("ana", item.Name.ToLowerInvariant()));

        var allActive = await GetListAsync("pageSize=100");
        Assert.True(anaSearch.TotalCount < allActive.TotalCount,
            "Search 'ana' não pode fazer corresponder todas as entidades pelo ramo do NIF");
    }
}
