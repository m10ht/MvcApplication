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
using Microsoft.AspNetCore.Identity;
using AppMvc.Areas.Blog.Models;

namespace AppMvc.Areas.Blog.Controllers
{
    [Area("Blog")]
    [Route("admin/blog/post/[action]/{id?}")]
    [Authorize(Roles = RoleName.Administrator + "," + RoleName.Editor)]
    public class PostController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly ILogger<PostController> _logger;

        public PostController(AppDbContext context, UserManager<AppUser> userManager, ILogger<PostController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }
        [TempData]
        public string StatusMessage {get; set;}
        // GET: Post
        public async Task<IActionResult> Index([FromQuery(Name = "p")] int currentPage, int pagesize)
        {
            // var appDbContext = _context.Posts.Include(p => p.Author);
            // return View(await appDbContext.ToListAsync());
            var posts = _context.Posts
                        .Include(p => p.Author)
                        .OrderByDescending(p => p.DateUpdated);

            var totalPosts = await _context.Posts.CountAsync();
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


            ViewBag.PagingModel = pagingModel;
            ViewBag.totalPosts = totalPosts;
            ViewBag.postIndex = (currentPage - 1) * pagesize;

            var postsInPage = await posts.Skip((currentPage - 1) * pagesize)
                        .Take(pagesize)
                        .Include(p => p.PostCategories)
                        .ThenInclude(pc => pc.Category)
                        .ToListAsync();
                        
            return View(postsInPage);
            // return View(await posts.ToListAsync());/

        }

        // GET: Post/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Posts == null)
            {
                return NotFound();
            }

            var post = await _context.Posts
                .Include(p => p.Author)
                .FirstOrDefaultAsync(m => m.PostId == id);
            if (post == null)
            {
                return NotFound();
            }

            return View(post);
        }

        // GET: Post/Create
        public async Task<IActionResult> Create()
        {
            // ViewData["AuthorId"] = new SelectList(_context.Users, "Id", "Id");
            // return View();
            var categories = await _context.Categories.ToListAsync();
            ViewData["categories"] = new MultiSelectList(categories, "Id", "Title");
            return View();
        }

        // POST: Post/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Description,Slug,Content,Published,DateCreated,DateUpdated,CategoryIDs")] CreatePostModel post)
        {
            var categories = await _context.Categories.ToListAsync();
            ViewData["categories"] = new MultiSelectList(categories, "Id", "Title");
        
            if (post.Slug == null) {
                post.Slug = App.Utilities.AppUtilities.GenerateSlug(post.Title);
            }

            if (await _context.Posts.AnyAsync(p => p.Slug == post.Slug)) {
                ModelState.AddModelError("Slug", "Nhập chuỗi URL khác");
                return View(post);
            }

            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(this.User);
                post.AuthorId = user.Id;

                _context.Add(post);
                if (post.CategoryIDs != null) {
                    foreach (var categoryid in post.CategoryIDs) {
                        _context.Add(new PostCategory {
                            CategoryID = categoryid,
                            Post = post
                        });
                    }
                }

                await _context.SaveChangesAsync();
                StatusMessage = "Đã tạo bài viết mới";
                return RedirectToAction(nameof(Index));
            }

            return View(post);
        }

        // GET: Post/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Posts == null)
            {
                return NotFound();
            }

            // var post = await _context.Posts.FindAsync(id);
            var post = await _context.Posts.Include(p => p.PostCategories).FirstOrDefaultAsync(p => p.PostId == id);
            if (post == null)
            {
                return NotFound();
            }
            var postEdit = new CreatePostModel() {
                PostId = post.PostId,
                Title = post.Title,
                Content = post.Content,
                Description = post.Description,
                Slug = post.Slug,
                Published = post.Published,
                DateCreated = post.DateCreated,
                DateUpdated = post.DateUpdated,
                CategoryIDs = post.PostCategories.Select(pc => pc.CategoryID).ToArray()
            };

            var categories = await _context.Categories.ToListAsync();
            ViewData["categories"] = new MultiSelectList(categories, "Id", "Title");
            // ViewData["AuthorId"] = new SelectList(_context.Users, "Id", "Id", post.AuthorId);
            return View(postEdit);
        }

        // POST: Post/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("PostId,Title,Description,Slug,Content,Published,DateCreated,DateUpdated,CategoryIDs")] CreatePostModel post)
        {
            
            if (id != post.PostId)
            {
                return NotFound();
            }

            var categories = await _context.Categories.ToListAsync();
            ViewData["categories"] = new MultiSelectList(categories, "Id", "Title");

            if (post.Slug == null) {
                post.Slug = App.Utilities.AppUtilities.GenerateSlug(post.Title);
            }

            if (await _context.Posts.AnyAsync(p => p.Slug == post.Slug && p.PostId != id)) {
                ModelState.AddModelError("Slug", "Nhập chuỗi URL khác");
                return View(post);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var postUpdate = await _context.Posts.Include(p => p.PostCategories).FirstOrDefaultAsync(p => p.PostId == id);
                    if (postUpdate == null)
                        return NotFound();

                    postUpdate.Title = post.Title;
                    postUpdate.Description = post.Description;
                    postUpdate.Content = post.Content;
                    postUpdate.Published = post.Published;
                    postUpdate.Slug = post.Slug;
                    postUpdate.DateCreated = post.DateCreated;
                    postUpdate.DateUpdated = post.DateUpdated;

                    // UPdate PostCategory
                    if (post.CategoryIDs == null)
                        post.CategoryIDs = new int[]{};
                    var oldCateIds = postUpdate.PostCategories.Select(pc => pc.CategoryID).ToArray();
                    var newCateIds = post.CategoryIDs;

                    var removeCatePosts = from postCate in postUpdate.PostCategories
                                            where(!newCateIds.Contains(postCate.CategoryID))
                                            select postCate;
                    _context.PostCategories.RemoveRange(removeCatePosts);

                    var addCateIds = from Cateid in newCateIds
                                    where !oldCateIds.Contains(Cateid)
                                    select Cateid;

                    foreach (var CateId in addCateIds) {
                        _context.PostCategories.Add(new PostCategory {
                            PostID = id,
                            CategoryID = CateId
                        });
                    }

                    
                    _context.Update(postUpdate);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PostExists(post.PostId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                StatusMessage = "Đã cập nhật bài viết";
                return RedirectToAction(nameof(Index));
            }
            ViewData["AuthorId"] = new SelectList(_context.Users, "Id", "Id", post.AuthorId);
            return View(post);
        }

        // GET: Post/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Posts == null)
            {
                return NotFound();
            }

            var post = await _context.Posts
                .Include(p => p.Author)
                .FirstOrDefaultAsync(m => m.PostId == id);
            if (post == null)
            {
                return NotFound();
            }

            return View(post);
        }

        // POST: Post/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Posts == null)
            {
                return Problem("Entity set 'AppDbContext.Posts'  is null.");
            }
            var post = await _context.Posts.FindAsync(id);
            if (post != null)
            {
                _context.Posts.Remove(post);
                StatusMessage = "Bạn đã xóa bài viết: " + post.Title;
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PostExists(int id)
        {
          return (_context.Posts?.Any(e => e.PostId == id)).GetValueOrDefault();
        }
    }
}
