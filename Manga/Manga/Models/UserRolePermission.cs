using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Manga.Models
{
    public class UserRolePermission
    {
        public int Id { get; set; }
        public string ApplicationUserId { get; set; }
        public string ApplicationRoleId { get; set; }
        public virtual ApplicationRole ApplicationRole { get; set;}
        public virtual ApplicationUser ApplicationUser { get; set; }
        public int PermissionId { get; set; }
        public virtual Permission Permission { get; set; }
    }
}