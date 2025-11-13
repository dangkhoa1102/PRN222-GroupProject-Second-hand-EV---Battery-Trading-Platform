using BLL.Services;
using DAL.Models;
using DAL.Repositories.Admin;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BLL.Configuration
{
    public static class AdminTransactionServiceRegistration
    {
        public static IServiceCollection AddAdminTransactionService(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<EVTradingPlatformContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("MyDbConnection")));

            services.AddScoped<IAdminTransactionService, AdminTransactionService>();
            services.AddScoped<IAdminReviewRepository, AdminReviewRepository>();


            return services;
        }
    }
}
