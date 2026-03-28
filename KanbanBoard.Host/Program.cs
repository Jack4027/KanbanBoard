using AutoMapper;
using KanbanBoard.Application.DependencyInjection;
using KanbanBoard.Application.Mapping;
using KanbanBoard.Host.Middleware;
using KanbanBoard.Infrastructure.DependencyInjection;
using KanbanBoard.Infrastructure.Hubs;
using KanbanBoard.Infrastructure.Identity;

using Microsoft.OpenApi.Models;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

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

builder.Services.AddSingleton(mapper);
builder.Services.AddControllers();

// Service containers
builder.Services.AddApplicationLayerServices(builder.Configuration);
builder.Services.AddInfrastructureLayerServices(builder.Configuration);

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter: Bearer {your token}"
    });
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
              .AllowCredentials(); // required for SignalR
    });
});

var app = builder.Build();

app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseCors("AllowAngular");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<KanbanHub>("/hubs/kanban");

app.Run();