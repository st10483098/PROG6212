using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RaceDay.Api.Data;

namespace RaceDay.Tests;

public class ApiFactory : WebApplicationFactory<Program>
{
    private readonly string _dbName = Guid.NewGuid().ToString();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureServices(services =>
        {
            var d = services.SingleOrDefault(x => x.ServiceType == typeof(DbContextOptions<RaceDayContext>));
            if (d != null) services.Remove(d);
            services.AddDbContext<RaceDayContext>(o => o.UseInMemoryDatabase(_dbName));
        });
    }
}
