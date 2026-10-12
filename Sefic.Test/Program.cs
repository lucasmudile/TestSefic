using Microsoft.EntityFrameworkCore;
using Sefic.Api.Application.Interfaces;
using Sefic.Api.Application.Services;
using Sefic.Api.Infrastructure.Persistence;
using Sefic.Api.Middleware;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<SeficDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SeficDb")));

builder.Services.AddScoped<IPartnerRepository, EfPartnerRepository>();
builder.Services.AddScoped<IPartnerService, PartnerService>();

builder.Services
    .AddControllers()
    .ConfigureApiBehaviorOptions(options =>
        options.InvalidModelStateResponseFactory = context =>
        {
            var message = context.ModelState
                .Where(entry => entry.Value?.Errors.Count > 0)
                .SelectMany(entry => entry.Value!.Errors.Select(error => error.ErrorMessage))
                .FirstOrDefault(message => !string.IsNullOrWhiteSpace(message))
                ?? "Pedido inválido.";

            return new ObjectResult(new ApiErrorResponse(StatusCodes.Status400BadRequest, "INVALID_REQUEST", message))
            {
                StatusCode = StatusCodes.Status400BadRequest
            };
        });

builder.Services.AddOpenApi();

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddSwaggerGen(options =>
        options.SwaggerDoc("v1", new Microsoft.OpenApi.OpenApiInfo
        {
            Title = "Sefic API — Clientes e Fornecedores",
            Version = "v1"
        }));
}

var app = builder.Build();

app.UseMiddleware<ErrorHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Sefic API v1");
    });
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();

public partial class Program;
