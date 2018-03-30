using Manga.Filter;
using Manga.Models;
using Microsoft.AspNet.Identity.Owin;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Manga.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        public UserController()
        {

        }
        public UserController(ApplicationUserManager userManager, ApplicationRoleManager roleManager)
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
            private set
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
        // GET: User
        [MVCAuthPermission("User.Read")]
        public async Task<ActionResult> Index()
        {
            return View(await UserManager.Users.ToListAsync());
        }
        // GET: User
        [MVCAuthPermission("User.Read")]
        public async Task<ActionResult> Detail(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var user = await UserManager.FindByIdAsync(id);
            var permissions = new List<Permission>();
            ViewBag.RoleNames = await UserManager.GetRolesAsync(user.Id);
            permissions = db.UserRolePermission.Where(s => s.ApplicationUserId == id).Select(c => c.Permission).Distinct().ToList();
            ViewBag.Permissions = permissions;
            ViewBag.PermissionsCount = permissions.Count();
            return View(user);
        }
        [MVCAuthPermission("User.Create")]
        public async Task<ActionResult> Create()
        {
            ViewBag.RoleId = new SelectList(await RoleManager.Roles.ToListAsync(), "Name", "Name");
            return View(new RegisterViewModel());
        }
       
        [HttpPost]
        [MVCAuthPermission("User.Create")]
        public async Task<ActionResult> Create(RegisterViewModel userViewModel, string[] selectedRoles)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = userViewModel.Email, Email = userViewModel.Email };
                var adminresult = await UserManager.CreateAsync(user, userViewModel.Password);

                //Add User to the selected Roles 
                if (adminresult.Succeeded)
                {
                    var result = await UserManager.AddToRolesAsync(user.Id, selectedRoles);
                    List<string> RoleIdList = new List<string>();
                    foreach(var itemRole in db.Roles)
                    {
                        if (selectedRoles.Contains(itemRole.Name))
                        {
                            RoleIdList.Add(itemRole.Id);
                        }
                    }
                    foreach(string roleId in RoleIdList)
                    {
                        foreach(var item in db.UserRolePermission)
                        {
                            if(item.ApplicationRoleId == roleId)
                            {
                                var newPermissionId = new UserRolePermission
                                {
                                    ApplicationUserId = user.Id,
                                    PermissionId = item.PermissionId
                                };
                                db.UserRolePermission.Add(newPermissionId);
                            }
                        }
                    }
                    db.SaveChanges();
                    if (!result.Succeeded)
                    {
                        ModelState.AddModelError("", result.Errors.First());
                        ViewBag.RoleId = new SelectList(await RoleManager.Roles.ToListAsync(), "Name", "Name");
                        return View();
                    }
                }
                else
                {
                    ModelState.AddModelError("", adminresult.Errors.First());
                    ViewBag.RoleId = new SelectList(RoleManager.Roles, "Name", "Name");
                    return View();

                }
                return RedirectToAction("Index");
            }
            ViewBag.RoleId = new SelectList(RoleManager.Roles, "Name", "Name");
            return View();
        }
        [MVCAuthPermission("User.Update")]
        public async Task<ActionResult> EditPermission(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var user = await UserManager.FindByIdAsync(id);
            if (user == null)
            {
                return HttpNotFound();
            }

            var userRoles = await UserManager.GetRolesAsync(user.Id);
            var permission = db.UserRolePermission.Where(s => s.ApplicationUserId == id).Select(x => x.PermissionId).ToList();
            var List = db.Permission.ToList().Select(x => new AdminUserView()
            {
                Selected = permission.Contains(x.Id),
                Text = x.Name,
                Value = x.Id
            }).ToList();
            return View(new AdminUserModel()
            {
                Id = user.Id,
                Email = user.Email,
                PermissionList = List
            });
        }

        [MVCAuthPermission("User.Update")]
        public async Task<ActionResult> Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var user = await UserManager.FindByIdAsync(id);
            if (user == null)
            {
                return HttpNotFound();
            }

            var userRoles = await UserManager.GetRolesAsync(user.Id);
            return View(new AdminUserModel()
            {
                Id = user.Id,
                Email = user.Email,
                RolesList = RoleManager.Roles.ToList().Select(x => new SelectListItem()
                {
                    Selected = userRoles.Contains(x.Name),
                    Text = x.Name,
                    Value = x.Name
                })

            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [MVCAuthPermission("User.Update")]
        public async Task<ActionResult> EditPermission([Bind(Include = "Email,Id")] AdminUserModel editUser, int[] SelectedPermission)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByIdAsync(editUser.Id);
                if (user == null)
                {
                    return HttpNotFound();
                }
                UpdatePermission(user.Id, SelectedPermission);
                return RedirectToAction("Index");
            }
            ModelState.AddModelError("", "Something failed.");
            var errors = ModelState.Values.SelectMany(v => v.Errors);
            return View();
        }
        //
        // POST: /Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [MVCAuthPermission("User.Update")]
        public async Task<ActionResult> Edit([Bind(Include = "Email,Id")] AdminUserModel editUser, string[] selectedRole)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByIdAsync(editUser.Id);
                if (user == null)
                {
                    return HttpNotFound();
                }

                user.UserName = editUser.Email;
                user.Email = editUser.Email;

                var userRoles = await UserManager.GetRolesAsync(user.Id);

                selectedRole = selectedRole ?? new string[] { };

                var result = await UserManager.AddToRolesAsync(user.Id, selectedRole.Except(userRoles).ToArray<string>());

                if (!result.Succeeded)
                {
                    ModelState.AddModelError("", result.Errors.First());
                    return View();
                }
                result = await UserManager.RemoveFromRolesAsync(user.Id, userRoles.Except(selectedRole).ToArray<string>());
                List<string> RoleIdList = new List<string>();
                foreach (var itemRole in db.Roles)
                {
                    if (selectedRole.Contains(itemRole.Name))
                    {
                        RoleIdList.Add(itemRole.Id);
                    }
                }
                foreach (string roleId in RoleIdList)
                {
                    foreach (var item in db.UserRolePermission)
                    {
                        if (item.ApplicationRoleId == roleId)
                        {
                            var newPermissionId = new UserRolePermission
                            {
                                ApplicationUserId = user.Id,
                                PermissionId = item.PermissionId
                            };
                            db.UserRolePermission.Add(newPermissionId);
                        }
                    }
                }
                db.SaveChanges();
                if (!result.Succeeded)
                {
                    ModelState.AddModelError("", result.Errors.First());
                    return View();
                }
                return RedirectToAction("Index");
            }
            ModelState.AddModelError("", "Something failed.");
            return View();
        }
        [MVCAuthPermission("User.Delete")]
        public async Task<ActionResult> Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var user = await UserManager.FindByIdAsync(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        //
        // POST: /Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [MVCAuthPermission("User.Delete")]
        public async Task<ActionResult> DeleteConfirmed(string id)
        {
            if (ModelState.IsValid)
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }

                var user = await UserManager.FindByIdAsync(id);
                if (user == null)
                {
                    return HttpNotFound();
                }
                DeleteUserPermission(id);
                var result = await UserManager.DeleteAsync(user);
                if (!result.Succeeded)
                {
                    ModelState.AddModelError("", result.Errors.First());
                    return View();
                }
                return RedirectToAction("Index");
            }
            return View();
        }
        private void UpdatePermission(string UserId, int[] Permissions)
        {
            var ListOfOldPermission = new HashSet<UserRolePermission>(db.UserRolePermission.Where(x => x.ApplicationUserId == UserId));
            foreach (var item in ListOfOldPermission)
            {
                db.UserRolePermission.Remove(item);
            }
            foreach (int newItemId in Permissions)
            {
                var newItem = new UserRolePermission
                {
                    ApplicationUserId = UserId,
                    PermissionId = newItemId
                };
                db.UserRolePermission.Add(newItem);
            }
            db.SaveChanges();
        }
        private void DeleteUserPermission(string UserId)
        {
            var listToRemove = db.UserRolePermission.Where(s => s.ApplicationUserId == UserId).ToList();
            foreach (var itemRemove in listToRemove)
            {
                db.UserRolePermission.Remove(itemRemove);
            }
            db.SaveChanges();
        }


    }
}