using CnabParser.Api;
using CnabParser.Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.MsSql;
using Xunit;

namespace CnabParser.Api.Tests;

public class CnabFileControllerFixture : IAsyncLifetime
{
    private readonly MsSqlContainer _msSqlContainer;
    public WebApplicationFactory<Program> Factory { get; private set; } = null!;
    public HttpClient HttpClient { get; private set; } = null!;

    public CnabFileControllerFixture()
    {
        _msSqlContainer = new MsSqlBuilder()
            .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
            .WithEnvironment("ACCEPT_EULA", "Y")
            .Build();
    }

    public async Task InitializeAsync()
    {
        await _msSqlContainer.StartAsync();

        var connectionString = _msSqlContainer.GetConnectionString();

        Factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Remove the existing DbContext registration
                    var descriptor = services.FirstOrDefault(d =>
                        d.ServiceType == typeof(DbContextOptions<CnabDbContext>));
                    if (descriptor != null)
                    {
                        services.Remove(descriptor);
                    }

                    // Add DbContext with Testcontainer connection string
                    services.AddDbContext<CnabDbContext>(options =>
                        options.UseSqlServer(connectionString));
                });
            });

        HttpClient = Factory.CreateClient();

        // Ensure database is created
        using (var scope = Factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<CnabDbContext>();
            await dbContext.Database.EnsureCreatedAsync();
        }
    }

    public async Task DisposeAsync()
    {
        HttpClient?.Dispose();
        Factory?.Dispose();

        await _msSqlContainer.StopAsync();
    }
}
