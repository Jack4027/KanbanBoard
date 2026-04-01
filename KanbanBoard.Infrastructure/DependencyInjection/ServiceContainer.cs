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
    public static class ServiceContainer
    {
        public static IServiceCollection AddInfrastructureLayerServices(
            this IServiceCollection services, IConfiguration config)
        {
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

            // Identity
            services.AddIdentityCore<AppUserIdentity>(opt =>
            {
                opt.Password.RequiredLength = 8;
                opt.Password.RequireDigit = true;
                opt.Password.RequireUppercase = true;
                opt.Password.RequireNonAlphanumeric = true;
            })
            .AddEntityFrameworkStores<KanbanDbContext>()
            .AddDefaultTokenProviders();

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
