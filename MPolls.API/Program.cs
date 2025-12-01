using System.Threading.RateLimiting;
using System.Threading.Tasks;
using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MPolls.API.Authentication;
using MPolls.API.Models;
using MPolls.API.Options;
using MPolls.API.Services;
using MPolls.Application;
using MPolls.Application.Common.Interfaces;
using MPolls.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var errorDetails = context.ModelState
                .Where(entry => entry.Value?.Errors.Count > 0)
                .ToDictionary(
                    entry => entry.Key,
                    entry => entry.Value!.Errors
                        .Select(error => string.IsNullOrWhiteSpace(error.ErrorMessage) ? "Invalid value." : error.ErrorMessage)
                        .ToArray());

            var payload = errorDetails.Count > 0 ? errorDetails : null;

            var response = ApiResponse<object>.Failure(
                new ApiError("validation_error", "One or more validation errors occurred."),
                StatusCodes.Status400BadRequest,
                payload);

            return new BadRequestObjectResult(response);
        };
    });

builder.Services.AddApplication();

builder.Services.AddPersistence(builder.Configuration);

builder.Services.Configure<FirebaseSettings>(builder.Configuration.GetSection("Firebase"));

var firebaseSettings = builder.Configuration.GetSection("Firebase").Get<FirebaseSettings>()
    ?? throw new InvalidOperationException("Firebase settings are not configured.");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = firebaseSettings.Authority;
        options.Audience = firebaseSettings.Audience;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = firebaseSettings.Authority,
            ValidateAudience = true,
            ValidAudience = firebaseSettings.Audience,
            ValidateLifetime = true
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                if (string.IsNullOrEmpty(context.Token)
                    && context.Request.Cookies.TryGetValue(AuthCookieDefaults.AccessTokenCookieName, out var token)
                    && !string.IsNullOrWhiteSpace(token))
                {
                    context.Token = token.Trim();
                }

                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddHttpClient<IFirebaseAuthService, FirebaseAuthService>(client =>
{
    client.BaseAddress = new Uri("https://identitytoolkit.googleapis.com/v1/");
});

builder.Services.AddSingleton<IFirebaseClientConfigProvider, FirebaseClientConfigProvider>();

builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy("AuthRateLimit", context =>
    {
        var remoteIp = context.Connection.RemoteIpAddress?.ToString();

        if (string.IsNullOrWhiteSpace(remoteIp))
        {
            remoteIp = context.Request.Headers[ForwardedHeadersDefaults.XForwardedForHeaderName].FirstOrDefault();
        }

        var partitionKey = !string.IsNullOrWhiteSpace(remoteIp)
            ? $"ip:{remoteIp}"
            : context.User.Identity?.Name ?? $"anon:{context.TraceIdentifier}";

        return RateLimitPartition.GetFixedWindowLimiter(partitionKey, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 5,
            Window = TimeSpan.FromMinutes(1),
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            QueueLimit = 0
        });
    });
});

// Allow the Blazor WebAssembly client to call this API during development
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowWebUI", policy =>
        policy.WithOrigins(allowedOrigins!)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());
});

builder.Services
    .AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(1, 0);
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.ReportApiVersions = true;
        options.ApiVersionReader = new UrlSegmentApiVersionReader();
    })
    .AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    });


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "MPolls API", Version = "v1" });
});

var app = builder.Build();

app.Logger.LogInformation("ASP.NET Core environment: {EnvironmentName}", app.Environment.EnvironmentName);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "MPolls API v1");
    });
}

app.UseCors("AllowWebUI");

app.UseHttpsRedirection();

app.UseRateLimiter();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
