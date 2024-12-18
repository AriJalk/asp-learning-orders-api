using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Orders.Infrastructure.DatabaseContexts;
using Orders.WebAPI;


namespace Orders.Tests.IntegrationTests
{
	public class CustomWebApplicationFactory : WebApplicationFactory<Program>
	{
		private readonly SqliteConnection _connection;

		public CustomWebApplicationFactory()
		{
			// Create an in-memory SQLite connection
			_connection = new SqliteConnection("Filename=:memory:");
		}

		protected override void ConfigureWebHost(IWebHostBuilder builder)
		{
			builder.UseEnvironment("Test");

			builder.ConfigureAppConfiguration((context, configBuilder) =>
			{
				configBuilder.AddUserSecrets<CustomWebApplicationFactory>();
			});

			builder.ConfigureServices(services =>
			{
				// Remove any existing DbContextOptions<ApplicationDbContext> registration
				var descriptor = services.SingleOrDefault(temp => temp.ServiceType == typeof(IDbContextOptionsConfiguration<ApplicationDbContext>));
				if (descriptor != null)
				{
					services.Remove(descriptor);
				}

				// Register the DbContext with the in-memory SQLite connection
				services.AddDbContext<ApplicationDbContext>(options =>
				{
					options.UseSqlite(_connection); // Use the in-memory SQLite connection
				});

				/*// Ensure the database schema is created (using EnsureCreated for simplicity)
				var serviceProvider = services.BuildServiceProvider();
				using (var scope = serviceProvider.CreateScope())
				{
					var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
					dbContext.Database.OpenConnection(); // Make sure the connection is open
					dbContext.Database.EnsureCreated(); // Create the schema if not already created
				}*/
			});
		}

		public async Task OpenConnection()
		{
			await _connection.OpenAsync();
			ResetDb();
		}

		public async Task CloseConnection()
		{
			await _connection.DisposeAsync(); 
		}

		private void ResetDb()
		{
			var sp = this.Services.CreateScope().ServiceProvider;
			using (var scope = sp.CreateScope())
			{
				var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
				db.Database.EnsureDeleted(); // Optional: Deletes the existing database
				db.Database.EnsureCreated(); // Recreate the database schema
			}
		}
	}

}
