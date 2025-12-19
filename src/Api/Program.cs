
using Application.Common.Interfaces;
using Domain.Entities;
using Hellang.Middleware.ProblemDetails;
using Infrastructure.Persistence;
using Infrastructure.Storage;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
 
 
 

var builder = WebApplication.CreateBuilder(args);
 
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());
});

 
// my db connection
 
var connStr = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connStr));

builder.Services.AddScoped<IAppDbContext>(sp =>
    sp.GetRequiredService<AppDbContext>());


//new
var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
        policy =>
        {
            policy.WithOrigins("http://localhost:4200")
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        });
});
 
var storageRoot = builder.Configuration.GetValue<string>("Storage:Root")
    ?? Path.Combine(AppContext.BaseDirectory, "storage");

var maxUpload = builder.Configuration.GetValue<long>("Storage:MaxUploadBytes", 104_857_600);
//1
builder.Services.AddSingleton<IStoragePathStrategy>(
    new DatePartitionedPathStrategy(storageRoot));
builder.Services.AddScoped<FileStorageService>(sp =>
{
    var db = sp.GetRequiredService<IAppDbContext>();
    var path = sp.GetRequiredService<IStoragePathStrategy>();

    long maxUpload = builder.Configuration.GetValue<long>("Storage:MaxUploadBytes");
    var allowedTypes = builder.Configuration
      .GetSection("Upload:AllowedTypes")
      .Get<string[]>()
      ?? new[] { "image/png", "image/jpeg", "application/pdf" };

    return new FileStorageService(db, path, maxUpload, allowedTypes);
});

//builder.Services.AddScoped<FileStorageService>(sp =>
//{
//    var db = sp.GetRequiredService<IAppDbContext>();
//    var path = sp.GetRequiredService<IStoragePathStrategy>();

//    long maxUpload = builder.Configuration.GetValue<long>("Storage:MaxUploadBytes");
//    var allowedTypes = builder.Configuration
//      .GetSection("Upload:AllowedTypes")
//      .Get<string[]>()
//      ?? new[] { "image/png", "image/jpeg", "application/pdf" };
//    return new FileStorageService(db, path, maxUpload, allowedTypes);
//});

builder.Host.UseSerilog((ctx, lc) => lc.ReadFrom.Configuration(ctx.Configuration));
 
builder.Services.AddControllers();

builder.Services.AddProblemDetails(options =>
{
    options.IncludeExceptionDetails = (ctx, ex) => true;
});
 
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("Jwt:Key missing");

builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
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
                Encoding.UTF8.GetBytes(jwtKey)
            ),
            RoleClaimType = ClaimTypes.Role,
            NameClaimType = JwtRegisteredClaimNames.Sub
        };
    });

 
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("UserOrAdmin", p => p.RequireRole("user", "admin"));
    options.AddPolicy("AdminOnly", p => p.RequireRole("admin"));
});
 
builder.Services.AddHealthChecks()
    .AddSqlServer(connStr, tags: new[] { "ready" })
    .AddCheck("fs_rw", () =>
    {
        try
        {
            Directory.CreateDirectory(storageRoot);
            var tmp = Path.Combine(storageRoot, "probe.tmp");
            File.WriteAllText(tmp, "ok");
            File.Delete(tmp);
            return HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(ex.Message);
        }
    }, tags: new[] { "ready" });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new() { Title = "File Storage API", Version = "v1" });

    // JWT Support
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter JWT token",
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});
builder.Services.AddControllers();

var app = builder.Build();
 
app.UseRouting();
app.UseCors("_myAllowSpecificOrigins");
app.UseAuthentication();
app.UseAuthorization();
app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();


app.UseProblemDetails();
app.UseSerilogRequestLogging();
app.UseCors("AllowAngular");

app.Use(async (ctx, next) =>
{
    if (!ctx.Request.Headers.ContainsKey("X-Correlation-ID"))
        ctx.Request.Headers["X-Correlation-ID"] = Guid.NewGuid().ToString();

    ctx.Response.Headers["X-Correlation-ID"] =
        ctx.Request.Headers["X-Correlation-ID"].ToString();

    await next();
});
// Health Endpoints

app.MapHealthChecks("/health/live");
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = r => r.Tags.Contains("ready")
});

app.Run();
