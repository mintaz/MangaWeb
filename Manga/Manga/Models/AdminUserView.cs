using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Manga.Models
{
    public class AdminUserView
    {
        public string RoleName { get; set; }
        public int Value { get; set; }
        public string Text { get; set; }
        public bool? Selected { get; set; }
    }
}