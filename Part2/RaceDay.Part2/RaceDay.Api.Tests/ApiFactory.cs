using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RaceDay.Api.Data;

namespace RaceDay.Api.Tests
{
    /// <summary>Test constants matching the seeded data.</summary>
    public static class TestData
    {
        public const string Password = "Password123!";
        public const string OrganiserCode = "TEST-ORGANISER-CODE";
        public const string Organiser1 = "thandeka.mokoena@raceday.co.za";   // owns events 1 and 2
        public const string Organiser2 = "sipho.naidoo@raceday.co.za";       // owns event 3
        public const string Participant1 = "lindiwe.dlamini@example.com";    // enrolled in events 1 and 2
    }

    /// <summary>Starts the API in memory with an in-memory database instead of SQL Server.</summary>
    public class ApiFactory : WebApplicationFactory<Program>
    {
        private readonly string _dbName = Guid.NewGuid().ToString();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureAppConfiguration((_, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["OrganiserSettings:InviteCode"] = TestData.OrganiserCode
                });
            });

            builder.ConfigureServices(services =>
            {
                // Remove the SQL Server registrations for RaceDayDbContext
                var toRemove = services
                    .Where(d => d.ServiceType.IsGenericType &&
                                d.ServiceType.GetGenericArguments().Contains(typeof(RaceDayDbContext)))
                    .ToList();
                foreach (var descriptor in toRemove)
                    services.Remove(descriptor);

                services.AddDbContext<RaceDayDbContext>(o => o.UseInMemoryDatabase(_dbName));
            });
        }

        protected override IHost CreateHost(IHostBuilder builder)
        {
            var host = base.CreateHost(builder);

            // Create the in-memory database (this also adds the seeded roles) and the sample data
            using var scope = host.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<RaceDayDbContext>();
            db.Database.EnsureCreated();
            DbSeeder.Seed(db);

            return host;
        }

        /// <summary>Creates a client that is already logged in as the given user.</summary>
        public async Task<HttpClient> LoggedInClientAsync(string email, string password = TestData.Password)
        {
            var client = CreateClient();
            var response = await client.PostAsJsonAsync("/api/auth/login", new { email, password });
            response.EnsureSuccessStatusCode();
            return client;
        }
    }
}