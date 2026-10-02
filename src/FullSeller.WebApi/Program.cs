using System.Text;
using FullSeller.Infrastructure;
using FullSeller.Infrastructure.Data;
using FullSeller.WebApi.Middleware;
using FullSeller.WebApi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ---------- Сервисы ----------

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "FULL SELLER API", Version = "v1" });

    var jwtScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Введите: Bearer {ваш JWT access-токен}",
        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" },
    };
    options.AddSecurityDefinition("Bearer", jwtScheme);
    options.AddSecurityRequirement(new OpenApiSecurityRequirement { { jwtScheme, Array.Empty<string>() } });
});

// Инфраструктура: репозитории (ADO.NET), UnitOfWork, внешние сервисы (SMS/Telegram/Push/File/JWT/Cargo).
builder.Services.AddFullSellerInfrastructure();

// Оркестрирующие сервисы уровня WebApi.
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<OrderService>();
builder.Services.AddScoped<PartnerService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("FullSellerClients", policy =>
    {
        var origins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
        if (origins.Length > 0)
            policy.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod();
        else
            policy.AllowAnyHeader().AllowAnyMethod().SetIsOriginAllowed(_ => true); // dev-режим: разрешить всё, если хосты не заданы
    });
});

var jwtSection = builder.Configuration.GetSection("Jwt");
builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        // MapInboundClaims=false + RoleClaimType="role": без этого ASP.NET Core переименовывает
        // короткий JWT-клейм "role" в длинный ClaimTypes.Role при валидации, и все проверки вида
        // principal.FindFirstValue("role") (IsPartner/IsAdmin/IsManagerOrAdmin — см. ClaimsPrincipalExtensions)
        // молча возвращают null, а [AdminOnly]/[PartnerOnly] всегда отдают 403 даже с верным токеном.
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtSection["Issuer"],
            ValidateAudience = true,
            ValidAudience = jwtSection["Audience"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSection["Key"] ?? string.Empty)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30),
            RoleClaimType = "role",
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// ---------- Применение миграций при старте (можно отключить в проде через конфиг) ----------

if (builder.Configuration.GetValue("Database:MigrateOnStartup", true))
{
    using var scope = app.Services.CreateScope();
    var migrationRunner = scope.ServiceProvider.GetRequiredService<MigrationRunner>();
    await migrationRunner.RunAsync();
}

// ---------- HTTP-конвейер ----------

app.UseFullSellerExceptionHandling();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/swagger/v1/swagger.json", "FULL SELLER API v1"));
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // wwwroot/uploads — файлы, загруженные через IFileStorage

app.UseCors("FullSellerClients");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
