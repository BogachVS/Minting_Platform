using Microsoft.EntityFrameworkCore;

namespace PiggyGame.Data;

public static class DependencyInjection
{
    public static void AddDatabase(this IServiceCollection serviceCollection, IConfiguration config)
    {
        serviceCollection.AddDbContext<IDbContext, PostgresqlDbContext>(opt =>
        {
            opt.UseNpgsql(config.GetConnectionString("PostgresqlContext"));
        });
    }
}