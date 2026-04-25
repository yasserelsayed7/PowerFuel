using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.FileProviders;
using PowerFuel.API.Services.FitnessCoach;
using PowerFuel.API.Services.Media;
using PowerFuel.Application.Media;
using PowerFuel.Infrastructure;
using PowerFuel.Infrastructure.Data;
using Swashbuckle.AspNetCore.Filters;

static void WriteStartupError(string context, Exception ex)
{
    try
    {
        var path = Path.Combine(AppContext.BaseDirectory, "logs");
        Directory.CreateDirectory(path);
        File.WriteAllText(
            Path.Combine(path, $"startup-error-{DateTime.UtcNow:yyyyMMdd-HHmmss}.txt"),
            $"[{context}]\n{ex}");
    }
    catch { }
}

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Logging
    builder.Logging.ClearProviders();
    builder.Logging.AddConsole();
    builder.Logging.AddDebug();

    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddHttpContextAccessor();

    // Swagger
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "PowerFuel API",
            Version = "v1"
        });

        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            In = ParameterLocation.Header,
            Name = "Authorization",
            Type = SecuritySchemeType.ApiKey,
            Description = "JWT Bearer token"
        });

        c.OperationFilter<SecurityRequirementsOperationFilter>();
    });

    // JWT
    var jwtSecret = builder.Configuration["Jwt:Secret"];

    if (string.IsNullOrEmpty(jwtSecret))
        throw new Exception("JWT Secret missing");

    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = builder.Configuration["Jwt:Issuer"],
                ValidAudience = builder.Configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtSecret)),
                ClockSkew = TimeSpan.Zero
            };
        });

    builder.Services.AddAuthorization();

    // Media
    builder.Services.Configure<MediaSettings>(
        builder.Configuration.GetSection(MediaSettings.SectionName));

    builder.Services.AddSingleton<IAssetsFileProvider, AssetsPhysicalFileProvider>();
    builder.Services.AddSingleton<IMediaUrlGenerator, MediaUrlGenerator>();

    // Infrastructure
    builder.Services.AddInfrastructure(builder.Configuration);

    // Fitness coach
    builder.Services.AddFitnessCoach(builder.Configuration);

    // CORS
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAll",
            p => p.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader());
    });

    var app = builder.Build();

    app.UseSwagger();
    app.UseSwaggerUI();

    // ── Assets / Static files ──────────────────────────────────────────────
    var configuredAssetsPath = builder.Configuration["Media:AssetsPhysicalPath"];

    // Log so we can see exactly what the server resolves
    app.Logger.LogInformation("ContentRootPath  : {Root}", app.Environment.ContentRootPath);
    app.Logger.LogInformation("BaseDirectory    : {Base}", AppContext.BaseDirectory);
    app.Logger.LogInformation("Configured assets: {Cfg}", configuredAssetsPath ?? "(empty)");

    // Resolve final path: config → content-root/assets fallback
    string assetsPath;
    if (!string.IsNullOrWhiteSpace(configuredAssetsPath))
    {
        assetsPath = configuredAssetsPath;
    }
    else
    {
        assetsPath = Path.Combine(app.Environment.ContentRootPath, "assets");
    }

    app.Logger.LogInformation("Resolved assets  : {Path}", assetsPath);

    // Create folder if missing — never crash startup over this
    if (!Directory.Exists(assetsPath))
    {
        try
        {
            Directory.CreateDirectory(assetsPath);
            app.Logger.LogInformation("Assets folder created at {Path}", assetsPath);
        }
        catch (Exception ex)
        {
            app.Logger.LogWarning(ex, "Could not create assets folder at {Path} — serving from fallback", assetsPath);
            assetsPath = Path.Combine(app.Environment.ContentRootPath, "assets");
            Directory.CreateDirectory(assetsPath); // content-root is always writable
        }
    }

    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(assetsPath),
        RequestPath = "/assets"
    });
    // ──────────────────────────────────────────────────────────────────────

    app.UseCors("AllowAll");
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();

    // Migration + Seed
    using (var scope = app.Services.CreateScope())
    {
        try
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await db.Database.MigrateAsync();

            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
            await DbSeed.SeedAsync(db, logger, assetsPath);
        }
        catch (Exception ex)
        {
            WriteStartupError("Migration", ex);
        }
    }

    await app.RunAsync();
}
catch (Exception ex)
{
    WriteStartupError("Fatal", ex);
    throw;
}