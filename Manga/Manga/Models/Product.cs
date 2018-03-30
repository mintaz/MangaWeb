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
        public string Author { get; set; }
        public string Description { get; set; }
        public string ImageCoverPath { get; set; }
        public virtual ICollection<Category> Categories { get; set; }
    }
}