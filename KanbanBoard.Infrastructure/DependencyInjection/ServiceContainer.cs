using KanbanBoard.Application.Interfaces.Hubs;
using KanbanBoard.Application.Interfaces.Identity;
using KanbanBoard.Domain.Interfaces.Repository;
using KanbanBoard.Infrastructure.Data;
using KanbanBoard.Infrastructure.Hubs;
using KanbanBoard.Infrastructure.Identity;
using KanbanBoard.Infrastructure.Repository;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Runtime.Intrinsics.X86;
using System.Text;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using static System.Net.WebRequestMethods;

namespace KanbanBoard.Infrastructure.DependencyInjection
{
    //Service container to be used to inject the infrastructure level services into program.cs, it includes the database context, repositories, authentication services, SignalR hub services and health checks.
    public static class ServiceContainer
    {
        public static IServiceCollection AddInfrastructureLayerServices(
            this IServiceCollection services, IConfiguration config)
        {
            // Database context configuration with SQL Server and retry logic for transient failures, allowing the application to automatically retry failed database operations up to 3 times with a delay of 5 seconds between each attempt,
            // improving resilience and reliability when connecting to the database.
            services.AddDbContext<KanbanDbContext>(opt =>
                opt.UseSqlServer(
                    config.GetConnectionString("DefaultConnection"),
                    sqlOptions => sqlOptions
                        .EnableRetryOnFailure(
                            maxRetryCount: 3,
                            maxRetryDelay: TimeSpan.FromSeconds(5),
                            errorNumbersToAdd: null)
                        .CommandTimeout(30)));

            // Repositories
            services.AddScoped<IBoardRepository, BoardRepository>();
            services.AddScoped<IColumnRepository, ColumnRepository>();
            services.AddScoped<ICardRepository, CardRepository>();
            services.AddScoped<IBoardMemberRepository, BoardMemberRepository>();

            // Auth Services
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<ITokenService, TokenService>();

            //Hub Services
            services.AddSignalR();
            services.AddScoped<IKanbanNotificationService, KanbanNotificationService>();

            // Identity config for user authentication and authorization, it sets up the password requirements for user accounts, such as a minimum length of 8 characters, the requirement for at least one digit, one uppercase letter, and one non-alphanumeric character.
            services.AddIdentityCore<AppUserIdentity>(opt =>
            {
                opt.Password.RequiredLength = 8;
                opt.Password.RequireDigit = true;
                opt.Password.RequireUppercase = true;
                opt.Password.RequireNonAlphanumeric = true;
            })
            //Use the kanbandbcontext as the context for ASP.NET Core Identity, allowing it to manage user authentication and authorization data within the same database context as the application's domain entities,
            //simplifying data management and ensuring consistency across the application.
            .AddEntityFrameworkStores<KanbanDbContext>()
            .AddDefaultTokenProviders();

            // Health checks configuration to monitor the health of the application, specifically checking the connectivity to the SQL Server database using the connection string defined in the application's configuration.
            services.AddHealthChecks()
            .AddSqlServer(
                connectionString: config.GetConnectionString("DefaultConnection")!,
                name: "sql-server",
                failureStatus: HealthStatus.Unhealthy,
                tags: new[] { "database", "sql" });

            return services;
        }
    }
}
