using App.Models.Blog;
using Microsoft.AspNetCore.Mvc;

namespace App.Components
{
    [ViewComponent]
    public class CategorySidebar : ViewComponent {
        public class CategorySidebarData {
            public List<Category>? Categories {get; set;}
            public int Level {get; set;}

            public string CategorySlug {get; set;}
        }
        public IViewComponentResult Invoke(CategorySidebarData categorySidebarData) {
            return View(categorySidebarData);
        }
    }
}