namespace Manga.Migrations
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;
    using System.Web;
    using Manga.Models;
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;
    using Microsoft.AspNet.Identity.Owin;

    internal sealed class Configuration : DbMigrationsConfiguration<Manga.Models.ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
            
        }

        protected override void Seed(ApplicationDbContext context)
        {
            context.Configuration.LazyLoadingEnabled = true;
            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));

            if (!roleManager.RoleExists("Admin"))
            {
                roleManager.Create(new IdentityRole("Admin"));
            }
            if (!roleManager.RoleExists("Smod"))
            {
                roleManager.Create(new IdentityRole("Smod"));
            }

            var user = new ApplicationUser { Email = "minhle1508@gmail.com" };


            if (userManager.FindByEmail("minhle1508@gmail.com") == null)
            {
                var result = userManager.Create(user, "P@ssw0rd");

                if (result.Succeeded)
                {
                    userManager.AddToRole(user.Id, "Admin");
                }

            }
            context.SaveChanges();
            var permissions = new List<Permission> {
                new Permission { Name ="ReadAbout" },
                new Permission{ Name = "ReadContact"},
                new Permission{Name = "ReadPermission"}
            };
            permissions.ForEach(s => context.Permission.AddOrUpdate(c => c.Name, s));
            context.SaveChanges();
            
            context.Category.AddOrUpdate(
                new Category()
                {
                    Title ="Kinh Dị",
                    Products = new List<Product>
                    {
                        new Product()
                        {
                            Name = "Doraemon"
                        },
                        new Product()
                        {
                            Name = "Dragonball"
                        }
                    }
                },
                 new Category()
                 {
                     Title = "Lãng Mạn",
                     Products = new List<Product>
                    {
                        new Product()
                        {
                            Name = "Đêm trong căn nhà hoang"
                        },
                        new Product()
                        {
                            Name = "Hồn về trong gió"
                        }
                    }
                 }
                );
            context.SaveChanges();


            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data. E.g.
            //
            //    context.People.AddOrUpdate(
            //      p => p.FullName,
            //      new Person { FullName = "Andrew Peters" },
            //      new Person { FullName = "Brice Lambson" },
            //      new Person { FullName = "Rowan Miller" }
            //    );
            //
        }
        public static void InitializeIdentityForEF(ApplicationDbContext db)
        {
            var roleManager = new RoleManager<ApplicationRole>(new RoleStore<ApplicationRole>(db));
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));

            const string name = "admin@admin.com";
            const string password = "Admin@123456";
            const string roleName = "Admin";

            //Create Role Admin if it does not exist
            var role = roleManager.FindByName(roleName);
            if (role == null)
            {
                role = new ApplicationRole(roleName);
                var roleresult = roleManager.Create(role);
            }

            var user = userManager.FindByName(name);
            if (user == null)
            {
                user = new ApplicationUser { UserName = name, Email = name };
                var result = userManager.Create(user, password);
                result = userManager.SetLockoutEnabled(user.Id, false);
            }

            // Add user admin to Role Admin if not already added
            var rolesForUser = userManager.GetRoles(user.Id);
            if (!rolesForUser.Contains(role.Name))
            {
                var result = userManager.AddToRole(user.Id, role.Name);
            }
        }
    }
}
