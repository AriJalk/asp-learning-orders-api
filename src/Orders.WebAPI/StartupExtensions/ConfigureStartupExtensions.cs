using Microsoft.EntityFrameworkCore;
using Orders.Core.Domain.RepositoryContracts;
using Orders.Core.ServiceContracts.OrderItems;
using Orders.Core.ServiceContracts.Orders;
using Orders.Core.Services.OrderItems;
using Orders.Core.Services.Orders;
using Orders.Infrastructure.DatabaseContexts;
using Orders.Infrastructure.Repositories;

namespace Orders.WebAPI.StartupExtensions
{
	public static class ConfigureStartupExtensions
	{
		public static IServiceCollection ConfigureServices(this IServiceCollection services, IConfiguration configuration)
		{
			services.AddScoped<IOrderGetterService, OrderGetterService>();
			services.AddScoped<IOrderAdderService, OrderAdderService>();
			services.AddScoped<IOrderFilterService, OrderFilterService>();
			services.AddScoped<IOrderUpdaterService, OrderUpdaterService>();
			services.AddScoped<IOrderDeleterService,  OrderDeleterService>();

			services.AddScoped<IOrderItemAdderService, OrderItemAdderService>();
			services.AddScoped<IOrderItemGetterService, OrderItemGetterService>();
			services.AddScoped<IOrderItemUpdaterService, OrderItemUpdaterService>();
			services.AddScoped<IOrderItemDeleterService, OrderItemDeleterService>();


			//services.AddScoped<IOrdersRepository, OrdersRepository>();
			//services.AddScoped<IOrderItemsRepository, OrderItemsRepository>();
			services.AddScoped<IUnitOfWork, UnitOfWork>();


			services.AddDbContext<ApplicationDbContext>(options =>
			{
				if (configuration.GetConnectionString("DefaultConnection") is string connection)
					options.UseSqlServer(connection);

			});

			

			return services;
		}
	}
}
