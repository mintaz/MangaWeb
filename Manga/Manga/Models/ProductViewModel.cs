using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Manga.Models
{
    public class ProductViewModel
    {
        public ProductViewModel()
        {
            Categories = new HashSet<Category>();
        }
        public int Id { get; set; }
        public string Name { get; set; }
        public string Author { get; set; }
        public string Description { get; set; }
        public string ImagePath { get; set; }
        public virtual ICollection<Category> Categories { get; set; }
        public IEnumerable<SelectListItem> CatalogriesList { get; set; }
    }
}