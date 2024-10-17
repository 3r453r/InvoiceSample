using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

namespace InvoiceSample.Persistence.Extensions
{
    public static class ApplicationBuilderExtension
    {
        public static IApplicationBuilder UseMigrations(this IApplicationBuilder builder)
        {
            using (var scope = builder.ApplicationServices.CreateScope())
            {
                var commandDb = scope.ServiceProvider.GetRequiredService<InvoiceSampleDbContext>();
                commandDb.Database.SetCommandTimeout(TimeSpan.FromMinutes(5));
                commandDb.Database.Migrate();
            }

            return builder;
        }
    }
}
