using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using QuanLyCLB.Application.Interfaces;
using QuanLyCLB.Infrastructure.Persistence;
using QuanLyCLB.Infrastructure.Services;
using QuanLyCLB.Infrastructure.Settings;

namespace QuanLyCLB.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtSettings>(configuration.GetSection("Authentication:Jwt"));

        services.AddDbContext<ClubManagementDbContext>(options =>
        {
            // Lấy chuỗi kết nối SQL Server từ cấu hình, mặc định sử dụng LocalDB cho môi trường dev
            var connectionString = configuration.GetConnectionString("Default")
                ?? "Server=(localdb)\\MSSQLLocalDB;Database=QuanLyCLB;Trusted_Connection=True;TrustServerCertificate=True;";

            // Khởi tạo DbContext với provider SQL Server cho tầng hạ tầng (Infrastructure)
            options.UseSqlServer(connectionString);
        });

        services.AddScoped<IInstructorService, InstructorService>();
        services.AddScoped<ITrainingClassService, TrainingClassService>();
        services.AddScoped<IScheduleService, ScheduleService>();
        services.AddScoped<IAttendanceService, AttendanceService>();
        services.AddScoped<IPayrollService, PayrollService>();
        services.AddScoped<IGoogleTokenValidator, GoogleTokenValidator>();
        services.AddSingleton<IJwtTokenService, JwtTokenService>();

        return services;
    }
}
