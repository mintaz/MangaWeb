using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Manga.Models
{
    public class Category
    {
        public Category()
        {
            Products = new HashSet<Product>();
        }
        public int Id { get; set; }
        public string Title { get; set; }
        
        public virtual ICollection<Product> Products { get; set; }
    }
}