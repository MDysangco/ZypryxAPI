using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Zyprix.Data.Interfaces;
using Zyprix.Data.Repositories;
using Zyprix.Services;
using Zyprix.Services.Interfaces;
using ZypryxAPI.Services.Caching;
using ZypryxAPI.Services.Flushing;
using Scrutor;

var builder = WebApplication.CreateBuilder(args);

var jwtKey = builder.Configuration["Jwt:Key"] ?? "zypryx-api";
var issuer = builder.Configuration["Jwt:Issuer"] ?? "zypryx-api";
var audience = builder.Configuration["Jwt:Audience"] ?? "zypryx-api";

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("UserOnly", policy =>
        policy.RequireClaim("role", "User"))
    .AddPolicy("TaskerOnly", policy =>
        policy.RequireClaim("role", "InternalService"));


// Add services to the container.
var conn = builder.Configuration.GetConnectionString("DefaultConnection") ?? "";

// Repositories
builder.Services.AddScoped<IKlineRepository>(sp => new KlineRepository(conn));
builder.Services.AddScoped<ICoinRepository>(sp => new CoinRepository(conn));
builder.Services.AddScoped<IReadingRepository>(sp => new ReadingRepository(conn));
builder.Services.AddScoped<IConfigurationRepository>(sp => new ConfigurationRepository(conn));

// Services
builder.Services.AddScoped<IKlineService, KlineService>();
builder.Services.AddScoped<ICoinService, CoinService>();
builder.Services.AddScoped<IReadingService, ReadingService>();
builder.Services.AddScoped<IConfigurationService, ConfigurationService>();


// Cached wrappers
builder.Services.Decorate<ICoinService, CachedCoinService>();
builder.Services.Decorate<IKlineService, CachedKlineService>();
builder.Services.Decorate<IReadingService, CachedReadingService>();

// Flushing
builder.Services.AddHostedService<CoinFlushService>();
builder.Services.AddHostedService<KlineFlushService>();
builder.Services.AddHostedService<ReadingFlushService>();


// Caching
builder.Services.AddMemoryCache();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
    });

builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
