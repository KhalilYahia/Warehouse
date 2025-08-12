
using Warehouse.Common;
using Warehouse.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;


namespace Warehouse.Data.SeedData
{
    public class MyDbContextSeed
    {
        public static void SeedData(ModelBuilder modelBuilder)
        {

        ////    modelBuilder.Entity<CustomRole>().HasData(
        ////           new CustomRole() { Id = "fab4fac1-c546-41de-aebc-a14da6895711", Name = Roles.AdminRole, ConcurrencyStamp = "1", NormalizedName = Roles.AdminRole }
        ////   );
           
        ////    modelBuilder.Entity<CustomRole>().HasData(
        ////         new CustomRole() { Id = "fab4fac1-c546-41de-aebc-a14da6895713", Name = Roles.DeveloperRole, ConcurrencyStamp = "3", NormalizedName = Roles.DeveloperRole }
        //// );
        ////    modelBuilder.Entity<CustomRole>().HasData(
        ////        new CustomRole() { Id = "fab4fac1-c546-41de-aebc-a14da6895714", Name = Roles.NormalUserRole, ConcurrencyStamp = "4", NormalizedName = Roles.NormalUserRole }
        ////);


        ////    var user = new CustomUser()
        ////    {
        ////        Id = "b74ddd14-6340-4840-95c2-db12554843e5",
        ////        UserName = "admin",
        ////        NormalizedUserName = ("Admin").ToUpper(),
        ////        Email = "admin@gmail.com",
        ////        NormalizedEmail = ("admin@gmail.com").ToUpper(),
        ////        LockoutEnabled = false,
        ////        PhoneNumber = "1234567890"
        ////    };
        ////    PasswordHasher<CustomUser> passwordHasher = new PasswordHasher<CustomUser>();
        ////    user.PasswordHash = passwordHasher.HashPassword(user, "1q2w!Q@W"); // old password Im$trongPassw0rd
        ////    modelBuilder.Entity<CustomUser>().HasData(user);


        ////    modelBuilder.Entity<IdentityUserRole<string>>().HasData(
        ////       new IdentityUserRole<string>() { RoleId = "fab4fac1-c546-41de-aebc-a14da6895711", UserId = "b74ddd14-6340-4840-95c2-db12554843e5" }
        ////       );

        ////    var Normaluser = new CustomUser()
        ////    {
        ////        Id = "b74ddd14-6340-4840-95c2-db12554843yy",
        ////        UserName = "normal",
        ////        NormalizedUserName = ("normal").ToUpper(),
        ////        Email = "normal@gmail.com",
        ////        NormalizedEmail = ("normal@gmail.com").ToUpper(),
        ////        LockoutEnabled = false,
        ////        PhoneNumber = "1234567890"
        ////    };

        ////    Normaluser.PasswordHash = passwordHasher.HashPassword(user, "123yyy123"); // old password Im$trongPassw0rd
        ////    modelBuilder.Entity<CustomUser>().HasData(Normaluser);


        ////    modelBuilder.Entity<IdentityUserRole<string>>().HasData(
        ////       new IdentityUserRole<string>() { RoleId = "fab4fac1-c546-41de-aebc-a14da6895714", UserId = "b74ddd14-6340-4840-95c2-db12554843yy" }
        ////       );


           
        }


    }
}
