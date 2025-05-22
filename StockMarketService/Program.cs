using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using StockMarketService.Services;   // <- namespace that holds AlphaVantageProvider
// -----------------------------------------------------------------------------
// 1) Build
// -----------------------------------------------------------------------------
var builder = WebApplication.CreateBuilder(args);

// -----------------------------------------------------------------------------
// 2)   Configuration (API key lives in appsettings.json -> "AlphaVantage:ApiKey")
// -----------------------------------------------------------------------------
builder.Services.AddHttpClient<IStockDataProvider, AlphaVantageProvider>();   // typed client

builder.Services
    .AddControllers()                                             // MVC pipeline
    .AddJsonOptions(o =>
        o.JsonSerializerOptions.PropertyNamingPolicy = null);     // keep PascalCase

// -----------------------------------------------------------------------------
// 3)   Swagger / OpenAPI
//    (be sure the Swashbuckle.AspNetCore 8.x package is referenced)
//    dotnet add package Swashbuckle.AspNetCore --version 8.1.1
// -----------------------------------------------------------------------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Stock Market Dashboard – Price-change API",
        Version = "v1"
    });
});

var app = builder.Build();

// -----------------------------------------------------------------------------
// 4)   Pipeline
// -----------------------------------------------------------------------------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// (Optional) redirect http → https while you are still on localhost
app.UseHttpsRedirection();

app.MapControllers();          // /api/stocks/analyze goes to StockController

app.Run();
