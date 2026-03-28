using KanbanBoard.Application.Interfaces.Hubs;
using KanbanBoard.Application.Interfaces.Identity;
using KanbanBoard.Domain.Interfaces.Repository;
using KanbanBoard.Infrastructure.Data;
using KanbanBoard.Infrastructure.Hubs;
using KanbanBoard.Infrastructure.Identity;
using KanbanBoard.Infrastructure.Repository;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace KanbanBoard.Infrastructure.DependencyInjection
{
    public static class ServiceContainer
    {
        public static IServiceCollection AddInfrastructureLayerServices(
            this IServiceCollection services, IConfiguration config)
        {
            // DbContext
            services.AddDbContext<KanbanDbContext>(opt =>
                opt.UseSqlServer(config.GetConnectionString("DefaultConnection")));

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

            return services;
        }
    }
}
