using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Orders.Infrastructure.DatabaseContexts;
using Orders.WebAPI;


namespace Orders.Tests.IntegrationTests
{
	public class CustomWebApplicationFactory : WebApplicationFactory<Program>
	{
		protected override void ConfigureWebHost(IWebHostBuilder builder)
		{
			base.ConfigureWebHost(builder);

			builder.UseEnvironment("Test");

			builder.ConfigureAppConfiguration((context, configBuilder) =>
			{
				//configBuilder.AddUserSecrets<CustomWebApplicationFactory>();
			});

			builder.ConfigureServices(services =>
			{
				var descriptor = services.SingleOrDefault(temp => temp.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
				if (descriptor != null)
				{
					services.Remove(descriptor);
				}

				services.AddDbContext<ApplicationDbContext>(options =>
				{
					//options.UseInMemoryDatabase("TestDatabase");
				});
			});
		}

	}
}
