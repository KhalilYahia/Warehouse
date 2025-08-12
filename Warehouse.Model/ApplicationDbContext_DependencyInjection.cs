using Warehouse.Domain.Entities;
using Warehouse.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;

namespace Warehouse.Model.ApplicationDbContext_DependencyInjection
{
    public static class ApplicationDbContext_DependencyInjection
    {
        public static void AddDbContext_Khalil(this IServiceCollection bld,string connectionString)
        {
            bld.AddDbContext<ApplicationDbContext>(options =>
            {
                //options.EnableSensitiveDataLogging();
                options.UseSqlServer(connectionString);
            });

        }

        /// <summary>
        /// Ensure database is created or migrated
        /// </summary>
        /// <param name="app"></param>
        public static void CreateDb_IfNotExist(this WebApplication app)
        {
            
            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                db.Database.Migrate();
            }
        }

        public static void AddIdentityOptions_Khalil(this IServiceCollection bld)
        {
            
            //bld.AddIdentity<CustomUser,CustomRole>(options =>
            //{
            //    //Password settings
            //    options.Password.RequireDigit = false;
            //    options.Password.RequiredLength = 6;
            //    options.Password.RequireNonAlphanumeric = false;
            //    options.Password.RequireUppercase = false;
            //    options.Password.RequireLowercase = false;
            //})
                    //.AddEntityFrameworkStores<ApplicationDbContext>()
                    //.AddDefaultTokenProviders();

            //.AddIdentity<CustomUser, CustomRole>(options =>
            // {
            //     // Password settings
            //     options.Password.RequireDigit = false;
            //     options.Password.RequiredLength = 6;
            //     options.Password.RequireNonAlphanumeric = false;
            //     options.Password.RequireUppercase = false;
            //     options.Password.RequireLowercase = false;


            // })

            //bld.Configure<IdentityOptions>(options =>
            //{
            //    // Password settings
            //    options.Password.RequireDigit = false;
            //    options.Password.RequiredLength = 6;
            //    options.Password.RequireNonAlphanumeric = false;
            //    options.Password.RequireUppercase = false;
            //    options.Password.RequireLowercase = false;


            //});
        }
    }
}
