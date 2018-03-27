namespace Manga.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    using System.Linq;
    using Manga.Models;
    using Microsoft.AspNet.Identity.EntityFramework;

    internal sealed class Configuration : DbMigrationsConfiguration<Manga.Models.ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
            
        }


        protected override void Seed(ApplicationDbContext context)
        {
            
            context.Permission.AddOrUpdate(
                p => p.Keyword,
                new Permission { Name= "Read Product", Keyword = "Product.Read" },
                new Permission { Name = "Update Product", Keyword = "Product.Update" },
                new Permission { Name = "Delete Product", Keyword = "Product.Delete" },
                new Permission { Name = "Create Product", Keyword = "Product.Create" },
                new Permission { Name = "Read Catagory", Keyword = "Catagory.Read" },
                new Permission { Name = "Create Catagory", Keyword = "Catagory.Create" },
                new Permission { Name = "Update Catagory", Keyword = "Catagory.Update" },
                new Permission { Name = "Delete Catagory", Keyword = "Catagory.Delete" },
                new Permission { Name = "Read User", Keyword = "User.Read" },
                new Permission { Name = "Update User", Keyword = "User.Update" },
                new Permission { Name = "Delete User", Keyword = "User.Delete" },
                new Permission { Name = "Create User", Keyword = "User.Create" },
                new Permission { Name = "Read Role", Keyword = "Role.Read" },
                new Permission { Name = "Update Role", Keyword = "Role.Update" },
                new Permission { Name = "Delete Role", Keyword = "Role.Delete" },
                new Permission { Name = "Create Role", Keyword = "Role.Create" }
                );


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
        //public static void InitializeIdentityForEF(ApplicationDbContext db)
        //{
        //    var roleManager = new RoleManager<ApplicationRole>(new RoleStore<ApplicationRole>(db));
        //    var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));

        //    const string name = "admin@admin.com";
        //    const string password = "Admin@123456";
        //    const string roleName = "Admin";

        //    //Create Role Admin if it does not exist
        //    var role = roleManager.FindByName(roleName);
        //    if (role == null)
        //    {
        //        role = new ApplicationRole(roleName);
        //        var roleresult = roleManager.Create(role);
        //    }

        //    var user = userManager.FindByName(name);
        //    if (user == null)
        //    {
        //        user = new ApplicationUser { UserName = name, Email = name };
        //        var result = userManager.Create(user, password);
        //        result = userManager.SetLockoutEnabled(user.Id, false);
        //    }

        //    // Add user admin to Role Admin if not already added
        //    var rolesForUser = userManager.GetRoles(user.Id);
        //    if (!rolesForUser.Contains(role.Name))
        //    {
        //        var result = userManager.AddToRole(user.Id, role.Name);
        //    }
        //}
    }
}
