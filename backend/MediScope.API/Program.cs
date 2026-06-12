using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MediScope.Common.Models;
using MediScope.Business.Services;
using MediScope.Business.Services.Interfaces;
using Microsoft.OpenApi.Models;
using System.Text.Json.Serialization;
using MediScope.Data.Repositories;
using MediScope.Business.Hubs;
using MediScope.Data;
using MediScope.API.Middleware;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
var builder = WebApplication.CreateBuilder(args);

// ── 1. DATABASE ──────────────────────────────────────────────────────
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// ── 2. JWT SETTINGS ──────────────────────────────────────────────────
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
builder.Services.Configure<JwtSettings>(jwtSettings);

var secretKey = jwtSettings["SecretKey"]!;

// ── 3. JWT AUTHENTICATION ────────────────────────────────────────────
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
                                       Encoding.UTF8.GetBytes(secretKey)),
        ClockSkew = TimeSpan.Zero   // no grace period on expiry
    };

    // Return 401 instead of redirecting to login page
    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = async context =>
        {
            var dbContext = context.HttpContext.RequestServices.GetRequiredService<AppDbContext>();

            // Extract claims from validated token
            var userIdClaim = context.Principal!.FindFirst(ClaimTypes.NameIdentifier)?.Value
               ?? context.Principal!.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            var tokenSessionId = context.Principal!
                .FindFirst("SessionId")?.Value;

            if (!Guid.TryParse(userIdClaim, out Guid userId))
            {
                context.Fail("Invalid user ID in token.");
                return;
            }

            if (string.IsNullOrEmpty(tokenSessionId))
            {
                context.Fail("SessionId claim missing.");
                return;
            }

            // Query ONLY the SessionId column — minimal DB hit
            var currentSessionId = await dbContext.Users
                .Where(u => u.Id == userId && !u.IsDeleted)
                .Select(u => u.CurrentSessionId.ToString())
                .FirstOrDefaultAsync();

            if (currentSessionId == null)
            {
                context.Fail("User not found.");
                return;
            }
            // Token's SessionId not equal to db then logged in at other place.
            if (currentSessionId != tokenSessionId)
            {
                context.Fail("Session has been invalidated. A new login was detected.");
            }

            // If they match — request proceeds normally
        },
        OnChallenge = context =>
        {
            context.HandleResponse();
            context.Response.StatusCode = 401;
            context.Response.ContentType = "application/json";

            var isSessionInvalid = context.AuthenticateFailure?.Message
                ?.Contains("Session") ?? false;

            var message = isSessionInvalid
                ? "Your session has been invalidated. Please log in again."
                : "Unauthorized. Please log in.";

            return context.Response.WriteAsync(
                System.Text.Json.JsonSerializer.Serialize(new
                {
                    success = false,
                    message,
                    sessionExpired = isSessionInvalid
                }));
        },
        OnForbidden = context =>
        {
            context.Response.StatusCode = 403;
            context.Response.ContentType = "application/json";
            return context.Response.WriteAsync(
                "{\"success\":false,\"message\":\"Forbidden. You do not have permission.\"}");
        },
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) &&
                path.StartsWithSegments("/api/hubs"))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
});

// ── 4. AUTHORIZATION POLICIES ────────────────────────────────────────
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("PatientOnly", policy => policy.RequireRole("Patient"));
    options.AddPolicy("DoctorOnly", policy => policy.RequireRole("Doctor"));
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("DoctorOrPatient", policy =>
        policy.RequireRole("Doctor", "Patient"));
    options.AddPolicy("DoctorOrAdmin", policy =>
        policy.RequireRole("Doctor", "Admin"));
});
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IMedicalDocumentRepository, MedicalDocumentRepository>();
// ── 5. SERVICES ──────────────────────────────────────────────────────
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IPatientService, PatientService>();
builder.Services.AddScoped<IDoctorService, DoctorService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IMetricDefinitionService, MetricDefinitionService>();
builder.Services.AddScoped<IHealthMetricService, HealthMetricService>();
builder.Services.AddScoped<IDoctorPatientService, DoctorPatientService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IPatientDashboardService, PatientDashboardService>();
builder.Services.AddScoped<IAdminDashboardService, AdminDashboardService>();
builder.Services.AddScoped<IDoctorDashboardService, DoctorDashboardService>();
builder.Services.AddScoped<IMedicalDocumentService, MedicalDocumentService>();
builder.Services.AddScoped<IPasswordResetService, PasswordResetService>();
// ── 6. CORS (for Angular frontend) ───────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// ── 7. CONTROLLERS ───────────────────────────────────────────────────
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    });
builder.Services.AddSignalR();
// ── 8. SWAGGER WITH JWT SUPPORT ──────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "MediScope API",
        Version = "v1"
    });

    // Add JWT input box to Swagger UI
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token. Example: Bearer eyJhbGci..."
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id   = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// ── BUILD ────────────────────────────────────────────────────────────
var app = builder.Build();

// ── MIDDLEWARE PIPELINE ──────────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAngular");
// Program.cs

app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<RealtimeHub>("/api/hubs/realtime");
app.Run();