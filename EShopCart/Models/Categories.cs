using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace EShopCart.Models
{
    public class Categories
    {
        [Key]
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }

        // Navigation property to link Category to many Products
        public ICollection<Product> Products { get; set; }
    }
}
