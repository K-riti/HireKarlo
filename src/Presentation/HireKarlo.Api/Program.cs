using System.Text;
using System.Threading.RateLimiting;
using HireKarlo.Application.Interfaces.AI;
using HireKarlo.Application.Interfaces.External;
using HireKarlo.Application.Interfaces.Repositories;
using HireKarlo.Application.Interfaces.Services;
using HireKarlo.AtsEngine;
using HireKarlo.Infrastructure.AI;
using HireKarlo.Infrastructure.Auth;
using HireKarlo.Infrastructure.External;
using HireKarlo.Infrastructure.Services;
using HireKarlo.Persistence;
using HireKarlo.Persistence.Repositories;
using HireKarlo.ResumeService;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddHttpClient();

// Configure Auth Settings
builder.Services.Configure<AuthSettings>(builder.Configuration.GetSection("Auth"));

// Add JWT Authentication (for our own tokens)
var jwtSecret = builder.Configuration["Auth:JwtSecret"] ?? "YourSuperSecretKeyAtLeast32Characters!";
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Auth:JwtIssuer"] ?? "HireKarlo",
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Auth:JwtAudience"] ?? "HireKarlo",
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

// Add Rate Limiting
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.User?.Identity?.Name ?? context.Request.Headers.Host.ToString(),
            factory: _ => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1)
            }));

    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

// Add DbContext - supports both SQL Server and PostgreSQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<HireKarloDbContext>(options =>
{
    // Detect PostgreSQL connection string (Render uses postgres://)
    if (connectionString?.Contains("postgres", StringComparison.OrdinalIgnoreCase) == true ||
        connectionString?.Contains("Host=", StringComparison.OrdinalIgnoreCase) == true)
    {
        options.UseNpgsql(connectionString);
    }
    else
    {
        options.UseSqlServer(connectionString);
    }
});

// Add Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IResumeRepository, ResumeRepository>();
builder.Services.AddScoped<IJobListingRepository, JobListingRepository>();
builder.Services.AddScoped<IMatchRepository, MatchRepository>();
builder.Services.AddScoped<IApplicationRepository, ApplicationRepository>();
builder.Services.AddScoped<IDreamCompanyRepository, DreamCompanyRepository>();
builder.Services.AddScoped<IContactRepository, ContactRepository>();
builder.Services.AddScoped<IRoadmapItemRepository, RoadmapItemRepository>();
builder.Services.AddScoped<IInterviewDigestEntryRepository, InterviewDigestEntryRepository>();

// AI Services Configuration - Use FREE providers
var useAzure = !string.IsNullOrEmpty(builder.Configuration["AzureOpenAI:ApiKey"]);

if (useAzure)
{
    // Azure OpenAI (if configured)
    builder.Services.Configure<AzureOpenAISettings>(
        builder.Configuration.GetSection("AzureOpenAI"));
    builder.Services.Configure<AzureAISearchSettings>(
        builder.Configuration.GetSection("AzureAISearch"));
    builder.Services.AddSingleton<IOpenAIService, AzureOpenAIService>();
    builder.Services.AddSingleton<IEmbeddingService, EmbeddingService>();
    builder.Services.AddSingleton<IVectorStoreService, AzureAISearchService>();
}
else
{
    // FREE alternatives: Groq + HuggingFace + PostgreSQL Vector Store
    // PostgresVectorStore persists embeddings across Render cold starts
    builder.Services.Configure<GroqSettings>(
        builder.Configuration.GetSection("Groq"));
    builder.Services.Configure<HuggingFaceSettings>(
        builder.Configuration.GetSection("HuggingFace"));

    builder.Services.AddHttpClient<IOpenAIService, GroqService>();
    builder.Services.AddHttpClient<IEmbeddingService, HuggingFaceEmbeddingService>();
    builder.Services.AddScoped<IVectorStoreService, PostgresVectorStore>();
}

builder.Services.Configure<JobFetchSettings>(
    builder.Configuration.GetSection("JobFetch"));

builder.Services.AddScoped<RAGOrchestrator>();
builder.Services.AddScoped<IAdvancedAIService, AdvancedAIService>();

// Add Application Services
builder.Services.AddScoped<IAtsScorer, AtsScorer>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IMatchingEngine, MatchingEngine>();
builder.Services.AddScoped<JobApplicationService>();

// Add Resume Services
builder.Services.AddScoped<IResumeParser, ResumeParser>();
builder.Services.AddScoped<IResumeGenerator, ResumeGenerator>();

// Add Job Fetch Service
builder.Services.AddScoped<IJobFetchService, JobFetchService>();

// Add Email Service
builder.Services.AddScoped<IEmailService, EmailService>();

// Add LinkedIn Optimizer Service
builder.Services.AddScoped<ILinkedInOptimizerService, LinkedInOptimizerService>();

// Add Mock Interview Service
builder.Services.AddScoped<IMockInterviewService, MockInterviewService>();

// Add Learning Path Service
builder.Services.AddScoped<ILearningPathService, LearningPathService>();

// Add Email Digest Service
builder.Services.AddScoped<IEmailDigestService, EmailDigestService>();

// Add MediatR
builder.Services.AddMediatR(cfg => {
    cfg.RegisterServicesFromAssembly(typeof(HireKarlo.Application.Interfaces.IUnitOfWork).Assembly);
});

// Add AutoMapper  
builder.Services.AddAutoMapper(cfg => 
{
    // AutoMapper configuration can be added here
}, typeof(HireKarlo.Application.Interfaces.IUnitOfWork).Assembly);

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazor", policy =>
    {
        policy.WithOrigins(
                builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? 
                new[] { "https://localhost:5001", "https://localhost:7001", "http://localhost:5173" })
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// Add Health Checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<HireKarloDbContext>();

var app = builder.Build();

// Auto-migrate database in production
if (!app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<HireKarloDbContext>();
    db.Database.Migrate();
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseCors("AllowBlazor");
app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();


app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
