using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using App.Models;
using App.Models.Blog;
using Microsoft.AspNetCore.Authorization;
using App.Data;

namespace AppMvc.Net.Areas.Blog.Controllers
{
    [Area("Blog")]
    [Route("admin/blog/category/[action]/{id?}")]
    [Authorize(Roles = RoleName.Administrator)]
    public class CategoryController : Controller
    {
        private readonly AppDbContext _context;

        public CategoryController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Category
        public async Task<IActionResult> Index()
        {
            // var appDbContext = _context.Categories.Include(c => c.ParentCategory);
            var qr = (from c in _context.Categories select c)
                    .Include(c => c.ParentCategory)
                    .Include(c => c.CategoryChildren);
            var categories = (await qr.ToListAsync())
                            .Where(c => c.ParentCategory == null)
                            .ToList();
            return View(categories);
            // return View(await appDbContext.ToListAsync());
        }

        // GET: Category/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Categories == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .Include(c => c.ParentCategory)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }
        private void CreateSelectItem(List<Category> src, List<Category> des, int level) {
            string prefix = string.Concat(Enumerable.Repeat("****", level));
            foreach (var category in src) {
                // category.Title = prefix + category.Title;
                des.Add(new Category(){
                    Id = category.Id,
                    Title = prefix + category.Title
                });
                if (category.CategoryChildren?.Count > 0)
                    CreateSelectItem(category.CategoryChildren.ToList(), des, level + 1);
            }
        }

        // GET: Category/Create
        // public IActionResult Create()
        public async Task<IActionResult> CreateAsync()
        {
            var qr = (from c in _context.Categories select c)
                    .Include(c => c.ParentCategory)
                    .Include(c => c.CategoryChildren);
            var categories = (await qr.ToListAsync())
                            .Where(c => c.ParentCategory == null)
                            .ToList();

            categories.Insert(0, new Category() {
                Id = -1,
                Title = "Không có danh mục cha"
            });
            var items = new List<Category>();
            CreateSelectItem(categories, items, 0);
            var selectList = new SelectList(items, "Id", "Title");

            ViewData["ParentCategoryId"] = selectList;
            // ViewData["ParentCategoryId"] = new SelectList(_context.Categories, "Id", "Slug");
            return View();
        }

        // POST: Category/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Description,Slug,ParentCategoryId")] Category category)
        {
            if (ModelState.IsValid)
            {
                if (category.ParentCategoryId == -1)
                    category.ParentCategoryId = null;

                _context.Add(category);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            var qr = (from c in _context.Categories select c)
                    .Include(c => c.ParentCategory)
                    .Include(c => c.CategoryChildren);
            var categories = (await qr.ToListAsync())
                            .Where(c => c.ParentCategory == null)
                            .ToList();

            categories.Insert(0, new Category() {
                Id = -1,
                Title = "Không có danh mục cha"
            });
            var items = new List<Category>();
            CreateSelectItem(categories, items, 0);
            var selectList = new SelectList(items, "Id", "Title");


            ViewData["ParentCategoryId"] = selectList;
            // ViewData["ParentCategoryId"] = new SelectList(_context.Categories, "Id", "Slug", category.ParentCategoryId);
            return View(category);
        }

        // GET: Category/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Categories == null)
            {
                return NotFound();
            }

            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            var qr = (from c in _context.Categories select c)
                    .Include(c => c.ParentCategory)
                    .Include(c => c.CategoryChildren);
            var categories = (await qr.ToListAsync())
                            .Where(c => c.ParentCategory == null)
                            .ToList();

            categories.Insert(0, new Category() {
                Id = -1,
                Title = "Không có danh mục cha"
            });
            var items = new List<Category>();
            CreateSelectItem(categories, items, 0);
            var selectList = new SelectList(items, "Id", "Title");

            ViewData["ParentCategoryId"] = selectList;
            return View(category);
        }

        // POST: Category/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,Slug,ParentCategoryId")] Category category)
        {
            if (id != category.Id)
            {
                return NotFound();
            }
            if (category.Id == category.ParentCategoryId)
                ModelState.AddModelError("", "Phải chọn danh mục cha khác");

            if (ModelState.IsValid && category.Id != category.ParentCategoryId)
            {
                try
                {
                    if (category.ParentCategoryId == -1)
                        category.ParentCategoryId = null;

                    _context.Update(category);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryExists(category.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }


            var qr = (from c in _context.Categories select c)
                    .Include(c => c.ParentCategory)
                    .Include(c => c.CategoryChildren);
            var categories = (await qr.ToListAsync())
                            .Where(c => c.ParentCategory == null)
                            .ToList();

            categories.Insert(0, new Category() {
                Id = -1,
                Title = "Không có danh mục cha"
            });
            var items = new List<Category>();
            CreateSelectItem(categories, items, 0);
            var selectList = new SelectList(items, "Id", "Title");

            
            ViewData["ParentCategoryId"] = selectList;
            return View(category);
        }

        // GET: Category/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Categories == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .Include(c => c.ParentCategory)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // POST: Category/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Categories == null)
            {
                return Problem("Entity set 'AppDbContext.Categories'  is null.");
            }
            // var category = await _context.Categories.FindAsync(id);
            var category = await _context.Categories
                            .Include(c => c.CategoryChildren)
                            .FirstOrDefaultAsync(c => c.Id == id);
            if (category == null)
            {
                return NotFound();
            }
            foreach (var cCategory in category.CategoryChildren) {
                cCategory.ParentCategoryId = category.ParentCategoryId;
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CategoryExists(int id)
        {
          return (_context.Categories?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
