using BLL.Services;
using DAL.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BLL.Configuration
{
    public static class AdminTransactionServiceRegistration
    {
        public static IServiceCollection AddAdminTransactionService(this IServiceCollection services, IConfiguration configuration)
        {
            // Đăng ký DbContext để BLL có thể truy cập vào DAL mà không cần Presentation reference trực tiếp
            services.AddDbContext<EVTradingPlatformContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("MyDbConnection")));

            // Đăng ký duy nhất AdminTransactionService
            services.AddScoped<IAdminTransactionService, AdminTransactionService>();

            return services;
        }
    }
}
