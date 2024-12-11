using Orders.WebAPI.Middleware;
using Orders.WebAPI.StartupExtensions;

namespace Orders.WebAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            builder.Services.ConfigureServices(builder.Configuration);

            var app = builder.Build();


            app.UseExceptionHandler("/error");
            app.UseExceptionHandlingMiddleware();

            // Configure the HTTP request pipeline.

            app.UseHsts();
            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();

        }
    }
}
