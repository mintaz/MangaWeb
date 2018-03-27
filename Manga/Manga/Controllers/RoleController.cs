using Manga.Filter;
using Manga.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Manga.Controllers
{
    [Authorize]
    public class RoleController : Controller
    {
        public RoleController()
        {

        }

        public RoleController(ApplicationUserManager userManager,
            ApplicationRoleManager roleManager)
        {
            UserManager = userManager;
            RoleManager = roleManager;
        }
        public ApplicationDbContext db = new ApplicationDbContext();
        private ApplicationUserManager _userManager;
        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            set
            {
                _userManager = value;
            }
        }

        private ApplicationRoleManager _roleManager;
        public ApplicationRoleManager RoleManager
        {
            get
            {
                return _roleManager ?? HttpContext.GetOwinContext().Get<ApplicationRoleManager>();
            }
            private set
            {
                _roleManager = value;
            }
        }
        [MVCAuthPermission("Role.Read")]
        public ActionResult Index()
        {
            return View(RoleManager.Roles);
        }
        [MVCAuthPermission("Role.Create")]
        public ActionResult Create()
        {
            var listOfPermission = new SelectList(db.Permission.ToList(), "Id", "Name");
            ViewBag.listOfPermissions = listOfPermission;
            return View();
        }
        [HttpPost]
        [MVCAuthPermission("Role.Create")]
        public async Task<ActionResult> Create(AdminRoleModel roleViewModel, params int[] selectPermissions)
        {
            if (ModelState.IsValid)
            {
                var role = new ApplicationRole(roleViewModel.Name);
                var roleresult = await RoleManager.CreateAsync(role);
                if (!roleresult.Succeeded)
                {
                    ModelState.AddModelError("", roleresult.Errors.First());
                    return View();
                }
                else
                {
                    UpdatePermission(role.Id, selectPermissions);
                    return RedirectToAction("Index");
                }


            }
            return View();
        }
        [MVCAuthPermission("Role.Update")]
        public async Task<ActionResult> Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var role = await RoleManager.FindByIdAsync(id);
            if (role == null)
            {
                return HttpNotFound();
            }
            var permission = db.UserRolePermission.Where(s => s.ApplicationRoleId == id).Select(x => x.PermissionId).ToList();
            var List = db.Permission.ToList().Select(x => new SelectListItem()
            {
                Selected = permission.Contains(x.Id),
                Text = x.Name,
                Value = x.Id.ToString()
            }).ToList();
            var roleModels = new RolePermissionView
            {
                Id = role.Id,
                Name = role.Name,
                permissionList = List
            };
            // AdminRoleModel roleModel = new AdminRoleModel { Id = role.Id, Name = role.Name };
            return View(roleModels);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [MVCAuthPermission("Role.Update")]
        public async Task<ActionResult> Edit([Bind(Include = "Name,Id")] AdminRoleModel roleModel, params int[] selectPermissions)
        {
            if (ModelState.IsValid)
            {
                var role = await RoleManager.FindByIdAsync(roleModel.Id);
                role.Name = roleModel.Name;
                UpdatePermission(role.Id, selectPermissions);
                await RoleManager.UpdateAsync(role);
                return RedirectToAction("Index");
            }
            return View();
        }
        [MVCAuthPermission("Role.Delete")]
        public async Task<ActionResult> Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var role = await RoleManager.FindByIdAsync(id);
            if (role == null)
            {
                return HttpNotFound();
            }
            return View(role);
        }

        //
        // POST: /Roles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [MVCAuthPermission("Role.Delete")]
        public async Task<ActionResult> DeleteConfirmed(string id, string deleteUser)
        {
            if (ModelState.IsValid)
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                var role = await RoleManager.FindByIdAsync(id);
                if (role == null)
                {
                    return HttpNotFound();
                }
                IdentityResult result;
                if (deleteUser != null)
                {
                    result = await RoleManager.DeleteAsync(role);
                }
                else
                {
                    DeleteRolePermission(role.Id);
                    result = await RoleManager.DeleteAsync(role);
                }

                if (!result.Succeeded)
                {
                    ModelState.AddModelError("", result.Errors.First());
                    return View();
                }
                return RedirectToAction("Index");
            }
            return View();
        }
        [MVCAuthPermission("Role.Read")]
        public async Task<ActionResult> Detail(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var role = await RoleManager.FindByIdAsync(id);
            // Get the list of Users in this Role
            var users = new List<ApplicationUser>();
            var permissions = new List<Permission>();
            // Get the list of Users in this Role
            foreach (var user in UserManager.Users.ToList())
            {
                if (await UserManager.IsInRoleAsync(user.Id, role.Name))
                {
                    users.Add(user);
                }
            }
            permissions = db.UserRolePermission.Where(s => s.ApplicationRoleId == id).Select(c => c.Permission).ToList();
            ViewBag.Permissions = permissions;
            ViewBag.PermissionsCount = permissions.Count();
            ViewBag.Users = users;
            ViewBag.UserCount = users.Count();
            return View(role);
        }
        private void UpdatePermission(string RoleId, int[] Permissions)
        {
            var ListOfOldPermission = new HashSet<UserRolePermission>(db.UserRolePermission.Where(x => x.ApplicationRoleId == RoleId));
            var ListOfUserOfRoles = db.Users.Where(c => c.Roles.Any(z => z.RoleId == RoleId)).Select(a => a.Id).ToList();
            foreach (var item in ListOfOldPermission)
            {
                db.UserRolePermission.Remove(item);
            }
            if (ListOfUserOfRoles.Count !=0)
            {
                foreach (int newItemId in Permissions)
                {
                    foreach (string userId in ListOfUserOfRoles)
                    {
                        var newItem = new UserRolePermission
                        {
                            ApplicationRoleId = RoleId,
                            PermissionId = newItemId,
                            ApplicationUserId = userId
                        };
                        db.UserRolePermission.Add(newItem);
                    }

                }
            }
            else
            {
                foreach (int newItemId in Permissions)
                {
                    var newItem = new UserRolePermission
                    {
                        ApplicationRoleId = RoleId,
                        PermissionId = newItemId
                    };
                    db.UserRolePermission.Add(newItem);
                }
            }
            db.SaveChanges();
        }
        private void DeleteRolePermission(string RoleId)
        {
            var listToRemove = db.UserRolePermission.Where(s => s.ApplicationRoleId == RoleId).ToList();
            foreach (var itemRemove in listToRemove)
            {
                db.UserRolePermission.Remove(itemRemove);
            }
            db.SaveChanges();
        }



    }
}