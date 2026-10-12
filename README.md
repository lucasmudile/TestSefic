# Sefic API — Teste técnico Fase 2 (API de clientes e fornecedores)

API ASP.NET Core para gerir clientes e fornecedores de uma única empresa, com **um registo por NIF**, concorrência otimista a nível de base de dados e validações completas.

Base de dados: **SQL Server** (LocalDB) via **EF Core**, com migrações e dados de exemplo.

## Versões usadas

| Componente | Versão |
|---|---|
| .NET SDK | 10.0.201 ( TargetFramework `net10.0`, LTS) |
| EF Core (SqlServer/Design/Tools) | 10.0.12 |
| Microsoft.AspNetCore.OpenApi | 10.0.12 |
| Swashbuckle.AspNetCore (Swagger UI) | 10.3.0 |
| SQL Server | LocalDB 2019 (15.0.4153.1) — qualquer SQL Server Express/Developer funciona |
| xUnit | 2.9.3 + Microsoft.AspNetCore.Mvc.Testing 10.0.12 |

## Estrutura

```
Sefic.Test/                 -> projeto da API (Sefic.Api.csproj)
  Domain/                   -> entidade Partner, PartnerType, normalização do NIF
  Application/              -> DTOs, validações, regras (PartnerService), exceções de domínio
  Infrastructure/Persistence/ -> SeficDbContext, EfPartnerRepository, seed
  Controllers/              -> PartnersController (4 endpoints)
  Middleware/               -> ErrorHandlingMiddleware (formato de erro consistente)
  Migrations/               -> migração inicial + dados de exemplo
Sefic.Api.Tests/            -> testes de integração xUnit contra SQL Server real (LocalDB)
```

Separação pedida pelo enunciado: API (Controllers/Middleware) → regras (Application/Domain) → persistência (Infrastructure).

## Pré-requisitos

- .NET SDK 10 (para abrir/executar no Visual Studio é preciso **Visual Studio 2026 (v18+)** — o VS 2022, mesmo no 17.14, suporta no máximo .NET 9 e falha com NETSDK1045. De alternar, a API corre na linha de comandos com `dotnet run` em qualquer versão)
- SQL Server LocalDB (instância `MSSQLLocalDB`) — [sqlserver-downloads](https://www.microsoft.com/en-us/sql-server/sql-server-downloads)
- Ferramenta EF (para migrações): `dotnet tool install --global dotnet-ef`

A connection string de desenvolvimento está em `Sefic.Test/appsettings.json`:

```
Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=SeficPartners;Integrated Security=True;...
```

```
É só alterar a string de conexão e apontar para o SQL server desejado.
```

A base de dados `SeficPartners` é criada automaticamente pela migração.

## Preparação

```bash
# a partir da raiz do repositório
cd Sefic.Test
dotnet ef database update        # cria SeficPartners, aplica esquema + dados de exemplo
```

## Execução

```bash
cd Sefic.Test
dotnet run --launch-profile http # http://localhost:5135
```

- **Swagger UI** (só em Development): `http://localhost:5135/swagger`
- OpenAPI JSON nativo (.NET): `http://localhost:5135/openapi/v1.json` (e `http://localhost:5135/swagger/v1/swagger.json` em formato Swagger 3.0)
- Exemplos HTTP prontos: `Sefic.Test/Sefic.Test.http` (REST Client do VS Code / Visual Studio)

## Endpoints

| Método | Rota | Sucesso | Erros |
|---|---|---|---|
| POST | `/api/partners` | 201 + Location + corpo com `version` | 400 validação, 409 NIF duplicado |
| GET | `/api/partners/{id}` | 200 (mesmo inativo) | 404 |
| GET | `/api/partners` | 200 `{items,totalCount,page,pageSize}` | 400 página/tipo inválidos |
| PUT | `/api/partners/{id}` | 200 + nova `version` | 400 pedido inválido, 404, 409 NIF duplicado ou version desatualizada |

Formato de erro consistente, sem stack traces:

```json
{ "status": 409, "errorCode": "DUPLICATE_TAX_NUMBER", "message": "..." }
```

## Regras de negócio implementadas

- **NIF**: remove espaços, pontos e hífenes; exige exatamente 9 dígitos ASCII; guardado como texto (zeros iniciais preservados). Único em todo o cadastro (índice UNIQUE na base de dados) — também para inativos e sob pedidos concorrentes, sem locks em memória.
- **Nome**: obrigatório, 2–100 caracteres após Trim.
- **Email**: opcional; vazio/espaços → `null`; senão Trim + validação de formato.
- **CreditLimit**: `decimal(18,2)`, 0 a 1 000 000; precisão superior a 2 casas é rejeitada (400), nunca arredondada.
- **Type**: `CUSTOMER`, `SUPPLIER` ou `BOTH`. No filtro da listagem: `CUSTOMER` inclui CUSTOMER+BOTH, `SUPPLIER` inclui SUPPLIER+BOTH, `BOTH` só BOTH. Filtros combinam por AND.
- **Listagem**: `search` por nome (parcial, sem distinção de maiúsculas) ou NIF normalizado; uma search só de separadores não faz casar todos os NIF. Ordenação por Name, depois Id. `TotalCount` contado antes de paginar. Filtros e paginação executados no SQL Server.
- **Concurrency**: `Version` = `rowversion` do SQL Server em Base64, opaco para o cliente. O UPDATE compara a versão na base de dados (`WHERE Version = @original`); a segunda escrita recebe 409 sem sobrescrever.
- **Datas**: `CreatedAt`/`UpdatedAt` em UTC definidos pelo servidor; `CreatedAt` nunca muda; Id/datas não são editáveis.
- **PUT** exige todos os campos (email pode ser null); atualização inválida nunca altera parcialmente o registo.

## Testes automatizados

```bash
dotnet test Sefic.Api.Tests
```

Correm contra **LocalDB real** (nada é mockado): cada execução cria uma base `SeficPartnersTest_<processo>` isolada com as migrações + seed, e elimina-a no fim. Requisito: instância `MSSQLLocalDB` iniciada (`sqllocaldb start MSSQLLocalDB`).

Cobrem os 7 cenários do enunciado, incluindo:

- normalização `" Ana Silva "`/`"001.234.567"` e duplicado `"001 234 567"` → 409; NIF reservado após desativação;
- BOTH visível nos três filtros, CUSTOMER fora do filtro SUPPLIER;
- desativar/reativar + acesso por Id a inativos;
- PUT com NIF alheio → 409 sem alterar dados;
- **dois POSTs simultâneos do mesmo NIF → um 201, um 409, existe um único registo** (índice único no SQL Server);
- **dois PUTs simultâneos com a mesma Version → um 200, um 409** (verificado com contexts/connexões independentes);
- 400s (precisão excessiva, NIF com letras, `page=0`, tipo inválido, version malformada) sem efeitos na base de dados;
- filtros combinados, TotalCount pré-paginação e desempate determinístico por Id.

## Notas

- Foi utilizado o SQL Server LocalDB para simplificar a configuração do ambiente de desenvolvimento e testes. Esta opção permite utilizar o motor SQL Server localmente, em conjunto com o Entity Framework Core e as suas migrations, mantendo a possibilidade de configurar outra instância do SQL Server através da connection string..
