using Manga.Models;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Manga.Startup))]
namespace Manga
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.CreatePerOwinContext(ApplicationDbContext.Create);
            app.CreatePerOwinContext<ApplicationUserManager>(ApplicationUserManager.Create);
            app.CreatePerOwinContext<ApplicationRoleManager>(ApplicationRoleManager.Create);
            app.CreatePerOwinContext<ApplicationSignInManager>(ApplicationSignInManager.Create);
            ConfigureAuth(app);
        }
    }
}
