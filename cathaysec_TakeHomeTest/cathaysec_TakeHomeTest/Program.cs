using System.Text.Json.Serialization;
using CathaySec.Api.Application;
using CathaySec.Api.Infrastructure;
using CathaySec.Api.Infrastructure.Authentication;
using CathaySec.Api.Logging;
using CathaySec.Api.Middleware;
using CathaySec.Api.Models;
using CathaySec.Api.Swagger.Examples;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi;
using NLog.Web;
using Swashbuckle.AspNetCore.Filters;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Host.UseNLog();

if (builder.Environment.IsDevelopment() &&
    string.Equals(
        Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER"),
        "true",
        StringComparison.OrdinalIgnoreCase))
{
    builder.Configuration["Kestrel:Certificates:Default:Path"] =
        "/home/app/.aspnet/https/cathaysec_TakeHomeTest.pfx";
    builder.Configuration["Kestrel:Certificates:Default:Password"] =
        builder.Configuration["Kestrel:Certificates:Development:Password"];
}

builder.Services.AddControllers(options => options.Filters.Add<ApiResponseFilter>())
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Where(entry => entry.Value?.Errors.Count > 0)
            .ToDictionary(
                entry => entry.Key,
                entry => entry.Value!.Errors.Select(error =>
                    string.IsNullOrWhiteSpace(error.ErrorMessage) ? "Invalid value." : error.ErrorMessage).ToArray());

        var validationLogger = context.HttpContext.RequestServices
            .GetRequiredService<IStructuredLogger<Program>>();
        validationLogger.Log(
            LogLevel.Warning,
            "API 輸入資料驗證失敗。",
            LogTag.Validation,
            new Dictionary<string, object?>
            {
                ["TraceId"] = context.HttpContext.TraceIdentifier,
                ["Method"] = context.HttpContext.Request.Method,
                ["Path"] = context.HttpContext.Request.Path.Value,
                ["ValidationFields"] = errors.Keys.ToArray()
            },
            "VALIDATION_ERROR");

        return new BadRequestObjectResult(ApiResponse<object>.Failure(
            "VALIDATION_ERROR", "Request validation failed.", errors)
            .WithTraceId(context.HttpContext.TraceIdentifier));
    };
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "證券交易資料查詢系統 API",
        Version = "v1",
        Description = "提供上市公司股票查詢、建立委託單與委託查詢功能。"
    });
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "cathaysec_TakeHomeTest.xml"));
    options.ExampleFilters();
    options.AddSecurityDefinition(ApiKeyAuthenticationHandler.SchemeName, new OpenApiSecurityScheme
    {
        Name = ApiKeyAuthenticationHandler.HeaderName,
        Type = SecuritySchemeType.ApiKey,
        In = ParameterLocation.Header,
        Description = "呼叫 API 所需的金鑰。開發環境範例值：cathaysec-dev-key。"
    });
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference(ApiKeyAuthenticationHandler.SchemeName, document)] = []
    });
});
builder.Services.AddSwaggerExamplesFromAssemblyOf<CreateOrderRequestExample>();
builder.Services.AddMemoryCache();
builder.Services.AddSingleton(typeof(IStructuredLogger<>), typeof(StructuredLogger<>));
builder.Services.AddSingleton<InMemoryStockRepository>();
builder.Services.AddSingleton<IStockRepository>(services =>
    services.GetRequiredService<InMemoryStockRepository>());
builder.Services.AddSingleton<IOrderRepository, InMemoryOrderRepository>();
builder.Services.AddScoped<IStockService, StockService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddHttpClient<IStockMasterDataClient, TwseStockMasterDataClient>(client =>
{
    client.BaseAddress = new Uri("https://openapi.twse.com.tw/");
    client.Timeout = TimeSpan.FromSeconds(15);
});
builder.Services.AddHostedService<StockMasterDataInitializer>();
builder.Services.AddAuthentication(ApiKeyAuthenticationHandler.SchemeName)
    .AddScheme<AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>(
        ApiKeyAuthenticationHandler.SchemeName, _ => { });
builder.Services.AddAuthorization();
builder.Services.AddHealthChecks();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<RequestLoggingMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health").AllowAnonymous();

app.Run();

public partial class Program;
