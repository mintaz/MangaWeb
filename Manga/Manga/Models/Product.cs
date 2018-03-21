using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Manga.Models
{
    public class Product
    {
        public Product()
        {
            Categories = new HashSet<Category>();
        }
        public int Id { get; set; }
        public string Name { get; set; }    
        public virtual ICollection<Category> Categories { get; set; }
    }
}