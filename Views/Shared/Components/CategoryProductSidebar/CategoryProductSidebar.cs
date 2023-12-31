using App.Models.Blog;
using App.Models.Product;
using Microsoft.AspNetCore.Mvc;

namespace App.Components
{
    [ViewComponent]
    public class CategoryProductSidebar : ViewComponent {
        public class CategorySidebarData {
            public List<CategoryProduct>? Categories {get; set;}
            public int Level {get; set;}

            public string CategorySlug {get; set;}
        }
        public IViewComponentResult Invoke(CategorySidebarData categorySidebarData) {
            return View(categorySidebarData);
        }
    }
}