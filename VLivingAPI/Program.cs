using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Repositories.Interfaces;
using Repositories.Repositories;
using Services.Interfaces;
using Services.Services;
using Services.Models;
using System.Text;
using VLivingAPI.Repositories.Data.Models;
using EVCS.Repositories.HuyCG.Interfaces;
using EVCS.Repositories.HuyCG.Basic;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsProduction())
{
    builder.Configuration
        .AddJsonFile("appsettings.Production.json", optional: true, reloadOnChange: true);
}

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure EmailSettings
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

// DbContext với connection từ appsettings - with better error handling
try 
{
    builder.Services.AddDbContext<VLivingDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking));
}
catch (Exception ex)
{
    Console.WriteLine($"Database context configuration error: {ex.Message}");
    if (!builder.Environment.IsProduction()) 
        throw;
}

// DI cho layers
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IEmailVerificationRepository, EmailVerificationRepository>();
builder.Services.AddScoped<IPasswordResetRepository, PasswordResetRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

// Generic Repository và UnitOfWork pattern
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Services layer
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IPropertyService, PropertyService>();
builder.Services.AddScoped<ILocationService, LocationService>();
builder.Services.AddScoped<IPostService, PostService>();
builder.Services.AddScoped<ISubscriptionPlanService, SubscriptionPlanService>();
builder.Services.AddScoped<IAdService, AdService>();
builder.Services.AddScoped<IAdRequestService, AdRequestService>();
builder.Services.AddScoped<IActivityService, ActivityService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IRoommateMatchService, RoommateMatchService>();
builder.Services.AddScoped<IRoommatePreferenceService, RoommatePreferenceService>();

// Background Services
builder.Services.AddHostedService<TokenCleanupService>();

// JWT Authentication with safer configuration
var jwtKey = builder.Configuration["Jwt:Key"];
if (!string.IsNullOrEmpty(jwtKey))
{
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false; // Cho development
            options.SaveToken = true;
            
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = builder.Configuration["Jwt:Issuer"],
                ValidAudience = builder.Configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                ClockSkew = TimeSpan.Zero
            };
        });
    
    builder.Services.AddSwaggerGen(option =>
    {
        option.DescribeAllParametersInCamelCase();
        option.ResolveConflictingActions(conf => conf.First());
        option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            In = ParameterLocation.Header,
            Description = "Please enter a valid token",
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            BearerFormat = "JWT",
            Scheme = "Bearer"
        });
        option.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                new string[]{}
            }
        });
    });
    
    builder.Services.AddAuthorization();
}
else
{
    Console.WriteLine("Warning: JWT Key is not configured - authentication disabled");
}

var app = builder.Build();

// Safer database initialization
if (!string.IsNullOrEmpty(builder.Configuration.GetConnectionString("DefaultConnection")))
{
    try
    {
        using (var scope = app.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<VLivingDbContext>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
            
            // Test connection with timeout
            var canConnect = await context.Database.CanConnectAsync();
            if (canConnect)
            {
                await context.Database.EnsureCreatedAsync();
                logger.LogInformation("Database ensured created successfully");
            }
            else
            {
                logger.LogWarning("Cannot connect to database - API will run without database");
            }
        }
    }
    catch (Exception ex)
    {
        var loggerFactory = app.Services.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger<Program>();
        logger.LogError(ex, "Database initialization error - continuing without database");
    }
}

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

// Only use auth if JWT is configured
if (!string.IsNullOrEmpty(jwtKey))
{
    app.UseAuthentication();
    app.UseAuthorization();
}

// Add simple health endpoints
app.MapGet("/", () => "VLivingAPI is working!");
app.MapGet("/health", () => new { 
    status = "healthy", 
    timestamp = DateTime.UtcNow,
    version = "v1.3.0-full-with-db"
});

app.MapControllers();

app.Run();
