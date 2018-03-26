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
        public async Task<ActionResult> Index()
        {
            return View(await UserManager.Users.ToListAsync());
        }
        public async Task<ActionResult> Detail(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var user = await UserManager.FindByIdAsync(id);
            var permissions = new List<Permission>();
            ViewBag.RoleNames = await UserManager.GetRolesAsync(user.Id);
            permissions = db.UserRolePermission.Where(s => s.ApplicationUserId == id).Select(c => c.Permission).ToList();
            ViewBag.Permissions = permissions;
            ViewBag.PermissionsCount = permissions.Count();
            return View(user);
        }
        public async Task<ActionResult> Create()
        {
            //Get the list of Roles
            var List = db.Permission.ToList().Select(x => new AdminUserView()
            {
                Text = x.Name,
                Value = x.Id,
                RoleName = GetRoleNameByPermission(x.Id)
            }).ToList();
            ViewBag.RoleId = new SelectList(await RoleManager.Roles.ToListAsync(), "Name", "Name");
            return View(new RegisterViewModel() {
                PermissionList = List
            });
        }
       
        [HttpPost]
        public async Task<ActionResult> Create(RegisterViewModel userViewModel, string[] selectedRoles, params int[] SelectedPermissions)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = userViewModel.Email, Email = userViewModel.Email };
                var adminresult = await UserManager.CreateAsync(user, userViewModel.Password);

                //Add User to the selected Roles 
                if (adminresult.Succeeded)
                {
                    var result = await UserManager.AddToRolesAsync(user.Id, selectedRoles);
                    UpdatePermission(user.Id, SelectedPermissions);
                    if (!result.Succeeded)
                    {
                        ModelState.AddModelError("", result.Errors.First());
                        ViewBag.RoleId = new SelectList(await RoleManager.Roles.ToListAsync(), "Name", "Name");
                        ViewBag.PermssionId = new SelectList(db.Permission.ToList(), "Id", "Name");
                        return View();
                    }
                }
                else
                {
                    ModelState.AddModelError("", adminresult.Errors.First());
                    ViewBag.RoleId = new SelectList(RoleManager.Roles, "Name", "Name");
                    ViewBag.PermssionId = new SelectList(db.Permission.ToList(), "Id", "Name");
                    return View();

                }
                return RedirectToAction("Index");
            }
            ViewBag.RoleId = new SelectList(RoleManager.Roles, "Name", "Name");
            ViewBag.PermssionId = new SelectList(db.Permission.ToList(), "Id", "Name");
            return View();
        }
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
            var permission = db.UserRolePermission.Where(s => s.ApplicationUserId == id).Select(x => x.PermissionId).ToList();
            var List = db.Permission.ToList().Select(x => new AdminUserView()
            {
                Selected = permission.Contains(x.Id),
                Text = x.Name,
                Value = x.Id,
                RoleName = GetRoleNameByPermission(x.Id)
            }).ToList();
            return View(new AdminUserModel()
            {
                Id = user.Id,
                Email = user.Email,
                RolesList = RoleManager.Roles.ToList().Select(x => new SelectListItem()
                {
                    Selected = userRoles.Contains(x.Name),
                    Text = x.Name,
                    Value = x.Name
                }),
                PermissionList = List
            });
        }

        public string GetRoleNameByPermission(int permissionId)
        {
            if (db.UserRolePermission.Any(x => x.PermissionId == permissionId))
            {
                var roleId = db.UserRolePermission.Where(x => x.PermissionId == permissionId).FirstOrDefault().ApplicationRoleId;
                if (roleId == null)
                {
                    return string.Empty;
                }
                else
                {
                    var roleName = db.Roles.FirstOrDefault(x => x.Id == roleId);
                    return roleName.Name;
                }
            }
            else
            {
                return string.Empty;
            }
            
        }
        //
        // POST: /Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Email,Id")] AdminUserModel editUser, string[] selectedRole, params int[] SelectedPermission)
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

                if (!result.Succeeded)
                {
                    ModelState.AddModelError("", result.Errors.First());
                    return View();
                }
                UpdatePermission(user.Id, SelectedPermission);
                return RedirectToAction("Index");
            }
            ModelState.AddModelError("", "Something failed.");
            return View();
        }
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
        //public string[] loadSelectRole(int[] permissions)
        //{
        //    List<string> listOfRole = new List<string>();
        //    foreach(var item in db.UserRolePermission)
        //    {
        //        if (permissions.Contains(item.Id))
        //        {
        //            if (!string.IsNullOrEmpty(item.ApplicationRoleId))
        //            {
        //                listOfRole.Add(item.ApplicationRoleId);
        //            }
        //        }
        //    }
        //    return listOfRole.Distinct().ToArray();
        //}
        private void UpdatePermission(string UserId, int[] Permissions)
        {

            var selectedPermission = new HashSet<int>(Permissions);
            var listOfPermission = new HashSet<int>(db.UserRolePermission.Where(c => c.ApplicationUserId == UserId).Select(x => x.PermissionId));
            var listToRemove = new HashSet<int>();
            foreach (var item in db.Permission)
            {
                if (selectedPermission.Contains(item.Id))
                {
                    if (!listOfPermission.Contains(item.Id))
                    {
                        var newPermission = new UserRolePermission
                        {
                            ApplicationUserId = UserId,
                            PermissionId = item.Id
                        };
                        db.UserRolePermission.Add(newPermission);
                    }
                }
                else
                {
                    if (listOfPermission.Contains(item.Id))
                    {
                        listToRemove.Add(item.Id);
                    }
                }
            }
            foreach (var itemRemove in listToRemove)
            {
                var newItem = db.UserRolePermission.First(s => s.ApplicationUserId == UserId && s.PermissionId==itemRemove);
                db.UserRolePermission.Remove(newItem);
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