using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Noor.Core.Options;
using Noor.Core.Security;
using Noor.Infrastructure.Data;
using Noor.Infrastructure.Services;

namespace Noor.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddNoorInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection is not configured.");

        services.Configure<JwtOptions>(configuration.GetSection("Jwt"));
        services.Configure<StoreOptions>(configuration.GetSection("Store"));

        services.AddDbContext<NoorDbContext>(options =>
            options.UseSqlServer(connectionString,
                sql => sql.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)));

        services.AddScoped<IPasswordHasher, Pbkdf2PasswordHasher>();
        services.AddScoped<ITokenService, JwtTokenService>();
        services.AddScoped<AuthService>();
        services.AddScoped<CatalogService>();
        services.AddScoped<CartService>();
        services.AddScoped<OrderService>();
        services.AddScoped<AccountService>();
        services.AddScoped<AdminService>();
        services.AddScoped<DbSeeder>();

        return services;
    }
}
