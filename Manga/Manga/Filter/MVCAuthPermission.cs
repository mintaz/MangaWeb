using Manga.Models;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using System.Web.Security;
using System.Web.Routing;

namespace Manga.Filter
{
    public class MVCAuthPermission : ActionFilterAttribute
    {
        ApplicationDbContext context = new ApplicationDbContext();
        private readonly string[] allowPermission;
        public MVCAuthPermission(params string[] permission)
        {
            this.allowPermission = permission;
        }
        public MVCAuthPermission()
        {

        }
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            string actionName = filterContext.ActionDescriptor.ActionName;
            string controllerName = filterContext.ActionDescriptor.ControllerDescriptor.ControllerName;
            if (!CheckAccessRight(actionName, controllerName))
            {

                filterContext.Result = new PartialViewResult
                {
                    ViewName = "_Unauthorized"
                };

            }
            else
            {
                base.OnActionExecuting(filterContext);
            }


        }
        private bool CheckAccessRight(string Action, string Controller)
        {
            bool authorize = false;
            foreach (string str in allowPermission)
            {
                string userId = HttpContext.Current.User.Identity.GetUserId();
                int perId = -1;
                if (context.Permission.Any(s => s.Name == str))
                {
                    perId = context.Permission.Single(s => s.Name == str).Id;
                }
                bool user = context.UserRolePermission.Any(s =>
                s.ApplicationUserId == userId && s.PermissionId == perId);
                if (user)
                {
                    authorize = true;
                }
            }
            return authorize;
        }


    }
}