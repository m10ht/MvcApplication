using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using App.Models;
using App.Models.Blog;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AppMvc.Net.Areas.Blog.Controllers
{
    [Area("Blog")]
    public class ViewPostController : Controller
    {
        private readonly ILogger<ViewPostController> _logger;
        private readonly AppDbContext _context;

        public ViewPostController(ILogger<ViewPostController> logger, AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }
        [Route("/post/{categoryslug?}")]
        public IActionResult Index(string categoryslug,[FromQuery(Name = "p")]int currentPage, int pagesize)
        {
            var categories = GetCategories();
            ViewBag.categories = categories;
            ViewBag.categoryslug = categoryslug;

            Category? category = null;
            if (!string.IsNullOrEmpty(categoryslug)) {
                category = _context.Categories.Where(c => c.Slug == categoryslug)
                                                .Include(c => c.CategoryChildren)
                                                .FirstOrDefault();
                if (category == null)
                    return NotFound("Không tìm thấy Category");
            }

            var posts = _context.Posts
                        .Include(p => p.Author)
                        .Include(p => p.PostCategories)
                        .ThenInclude(p => p.Category)
                        .AsQueryable();

            posts.OrderByDescending(p => p.DateUpdated);

            if (category != null) {
                var ids = new List<int>();
                category.ChildCategoryIDs(ids, null);
                ids.Add(category.Id);

                posts = posts.Where(p => p.PostCategories.Where(pc => ids.Contains(pc.CategoryID)).Any());
            }

            var totalPosts = posts.Count();
            if (pagesize <= 0)
                pagesize = 10;
            int countPages = (int)Math.Ceiling((double)totalPosts / pagesize);
            
            if (currentPage > countPages)
                currentPage = countPages;
            if (currentPage < 1)
                currentPage = 1;
            
            var pagingModel = new PagingModel() {
                currentpage = currentPage,
                countpages = countPages,
                generateUrl = (pageNumber) => Url.Action("Index", new {
                    p = pageNumber,
                    pagesize = pagesize
                })
            };
            var postsInPage = posts.Skip((currentPage - 1) * pagesize)
                        .Take(pagesize);

            ViewBag.PagingModel = pagingModel;
            ViewBag.totalPosts = totalPosts;

            ViewBag.category = category;
            return View(postsInPage.ToList());
        }
        [Route("/post/{postslug}.html")]
        public IActionResult Details(string postslug) {
            var categories = GetCategories();
            ViewBag.categories = categories;

            var posts = _context.Posts.Where(p => p.Slug == postslug)
                                        .Include(p => p.Author)
                                        .Include(p => p.PostCategories)
                                        .ThenInclude(p => p.Category)
                                        .FirstOrDefault();
            if (posts == null)
                return NotFound("Khong tim thay");

            Category? category = posts.PostCategories?.FirstOrDefault()?.Category;
            ViewBag.category = category;

            var otherPosts = _context.Posts.Where(p => p.PostCategories.Any(c => c.Category.Id == category.Id))
                                            .Where(p => p.PostId != posts.PostId)
                                            .OrderByDescending(p => p.DateUpdated)
                                            .Take(5);
            ViewBag.otherPosts = otherPosts;

            return View(posts);
        }

        private List<Category> GetCategories() {
            var categories = _context.Categories
                            .Include(c => c.CategoryChildren)
                            .AsEnumerable()
                            .Where(c => c.ParentCategory == null)
                            .ToList();
            return categories;
        }   
    }
}