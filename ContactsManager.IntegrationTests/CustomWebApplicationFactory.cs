using Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CRUDTests
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            base.ConfigureWebHost(builder);

            builder.UseEnvironment("Test");

            //All the services that are added in the Program.cs are available in the IServiceCollection -> services
            builder.ConfigureTestServices(services =>
            {
                //services.RemoveAll<ApplicationDbContext>();
                //services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
                //services.RemoveAll<DbContextOptions>();

                ////Service descriptor represents the type of the service and its lifetime
                //var descriptor = services.SingleOrDefault(temp =>
                //temp.ServiceType == typeof(DbContextOptions<ApplicationDbContext>)
                //);

                //if (descriptor != null )
                //{
                //    services.Remove(descriptor);
                //}

                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseInMemoryDatabase("DbForTesting");
                });
            });
        }
    }
}
