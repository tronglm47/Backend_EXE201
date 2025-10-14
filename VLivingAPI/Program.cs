using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Repositories;
using Repositories.Basic;
using Services;
using Services.Models;
using Services.Utils;
using System.Text;
using Repositories.Data;

var builder = WebApplication.CreateBuilder(args);

// Configuration is automatically loaded in this order:
// 1. appsettings.json
// 2. appsettings.{Environment}.json (overrides appsettings.json)
// No need to manually add them again as WebApplicationBuilder does this by default

// Debug: Print connection string to verify which one is being used
// var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
// Console.WriteLine($"=== ENVIRONMENT: {builder.Environment.EnvironmentName} ===");
// Console.WriteLine($"=== CONNECTION STRING: {connectionString} ===");

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("https://frontend-dashboard-exe-201.vercel.app", "http://localhost:3000") // Thay đổi theo domain frontend của bạn
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddProfile<AutoMapperProfile>();
});

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
builder.Services.AddScoped<IBookingRepository, BookingRepository>();
builder.Services.AddScoped<IReviewRepository, ReviewRepository>();

// Generic Repository và UnitOfWork pattern
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Services layer
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ICloudStorageService, CloudStorageService>();
//
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IPostService, PostService>();
builder.Services.AddScoped<IUtilityService, UtilityService>();
builder.Services.AddScoped<IPostUtilityService, PostUtilityService>();
builder.Services.AddScoped<ISubdivisionService, SubdivisionService>();
builder.Services.AddScoped<IBuildingService, BuildingService>();
builder.Services.AddScoped<IApartmentService, ApartmentService>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<ILocationService, LocationService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
//

// SignalR
builder.Services.AddSignalR();

// HttpClient for LocationService
builder.Services.AddHttpClient<ILocationService, LocationService>();

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
        
        // Support for file uploads with custom schema mapping
        option.MapType<IFormFile>(() => new OpenApiSchema
        {
            Type = "string",
            Format = "binary"
        });
        
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

app.UseCors();

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

// Map SignalR hub
app.MapHub<VLivingAPI.Hubs.LocationTrackingHub>("/locationHub");

app.Run();
