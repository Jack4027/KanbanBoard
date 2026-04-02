using AutoMapper;
using KanbanBoard.Application.DependencyInjection;
using KanbanBoard.Application.Mapping;
using KanbanBoard.Host.Middleware;
using KanbanBoard.Infrastructure.DependencyInjection;
using KanbanBoard.Infrastructure.Hubs;
using KanbanBoard.Infrastructure.Identity;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Threading.RateLimiting;

// The main entry point of the application, responsible for configuring and starting the web host. It sets up various services and middleware components,
// including Serilog for logging, AutoMapper for object mapping, rate limiting to prevent abuse, Swagger for API documentation, CORS policies for cross-origin requests, and health checks for monitoring the application's health status.
// The application is configured to handle exceptions globally using a custom error handling middleware and to serve API endpoints defined in controllers. Additionally, it maps a SignalR hub for real-time communication with clients.
var builder = WebApplication.CreateBuilder(args);

// Serilog configuration
builder.Host.UseSerilog((context, config) =>
{
    config.ReadFrom.Configuration(context.Configuration);
});

// AutoMapper
var loggerFactory = LoggerFactory.Create(b => b.AddSerilog());
var mapper = new MapperConfiguration(cfg =>
{
    cfg.AddMaps(typeof(KanbanMapping).Assembly);
    cfg.AddMaps(typeof(IdentityMappingProfile).Assembly);
}, loggerFactory).CreateMapper();

// Register AutoMapper as a singleton service, allowing it to be injected into other components of the application where object mapping is required.
//Singleton lifetime is appropriate for AutoMapper since it is thread-safe and can be shared across the entire application without the need for multiple instances, improving performance and reducing memory usage.
builder.Services.AddSingleton(mapper);

//Add controllers 
builder.Services.AddControllers();

// Service containers
builder.Services.AddApplicationLayerServices(builder.Configuration);
builder.Services.AddInfrastructureLayerServices(builder.Configuration);

// Add hsts as a security measure to enforce secure connections and protect against man in the middle attacks,
// it instructs browsers to only communicate with the server over HTTPS for a specified duration (1 year in this case) and includes subdomains to ensure comprehensive protection across the entire domain and its subdomains.
// The preload option allows the site to be included in browsers' HSTS preload lists, further enhancing security by ensuring that even first-time visitors are protected from insecure connections.
builder.Services.AddHsts(options =>
{
    options.MaxAge = TimeSpan.FromDays(365);
    options.IncludeSubDomains = true;
    options.Preload = true;
});

// Rate limiting, it defines two fixed window limiters: one for authentication-related endpoints (with a limit of 5 requests per minute) and another for general endpoints (with a limit of 100 requests per minute).
// If a client exceeds the defined limits, they will receive a 429 Too Many Requests response with a message indicating that they should try again later. This helps to prevent abuse and ensure fair usage of the API.
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("auth", limiter =>
    {
        limiter.PermitLimit = 5;
        limiter.Window = TimeSpan.FromMinutes(1);
        limiter.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiter.QueueLimit = 0;
    });

    options.AddFixedWindowLimiter("general", limiter =>
    {
        limiter.PermitLimit = 100;
        limiter.Window = TimeSpan.FromMinutes(1);
        limiter.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiter.QueueLimit = 0;
    });

    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        await context.HttpContext.Response.WriteAsJsonAsync(
            new { message = "Too many requests. Please try again later." },
            cancellationToken);
    };
});

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    // Configure Swagger to use JWT Bearer authentication, allowing clients to authenticate and authorize requests using JWT tokens when interacting with the API endpoints through the Swagger UI.
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter: Bearer {your token}"
    });
    // Add a security requirement to Swagger, specifying that the "Bearer" security scheme defined above is required for accessing the API endpoints.
    // This ensures that clients must provide a valid JWT token in the Authorization header when making requests through the Swagger UI, enforcing authentication and authorization for protected endpoints.
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
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
            Array.Empty<string>()
        }
    });
});

// CORS
builder.Services.AddCors(opt =>
{
    opt.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // required for SignalR as it uses cookies for authentication, allowing the Angular frontend to establish a connection with the SignalR hub and receive real-time updates without running into cross-origin issues.
    });
});

var app = builder.Build();

// Global error handling middleware, it catches exceptions thrown during request processing and returns appropriate HTTP responses based on the exception type, ensuring consistent error handling and responses across the application.
app.UseMiddleware<ErrorHandlingMiddleware>();

// CORS and rate limiting middleware, it applies the defined CORS policy to allow cross-origin requests from the specified Angular frontend and enforces rate limits to prevent abuse and ensure fair usage of the API.
app.UseCors("AllowAngular");
app.UseRateLimiter();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    //Use hsts for production environment to enforce secure connections
    app.UseHsts();
}

app.UseHttpsRedirection();

// Authentication and authorization middleware, it enables authentication and authorization mechanisms in the application, allowing clients to authenticate and authorize requests based on their credentials and permissions when accessing protected API endpoints.
app.UseAuthentication();
app.UseAuthorization();

// Map controllers, health checks, and SignalR hub, it defines the routing for API endpoints, health check endpoint for monitoring the application's health status, and a SignalR hub for real-time communication with clients.
app.MapControllers();
app.MapHealthChecks("/health", new HealthCheckOptions
{
    //Custom response writer for health checks, it formats the health check report into a JSON response that includes the overall status,
    //individual check statuses, descriptions, durations, and total duration of the health checks, providing detailed information about the health status of the application to clients.
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";

        var result = new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description,
                duration = e.Value.Duration.TotalMilliseconds + "ms"
            }),
            totalDuration = report.TotalDuration.TotalMilliseconds + "ms"
        };

        await context.Response.WriteAsJsonAsync(result);
    }
});
app.MapHub<KanbanHub>("/hubs/kanban");

app.Run();