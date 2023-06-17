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
using AppMvc.Areas.Product.Models;
using App.Models.Product;
using System.ComponentModel.DataAnnotations;

namespace AppMvc.Areas.Product.Controllers
{
    [Area("Product")]
    [Route("admin/productmanage/[action]/{id?}")]
    [Authorize(Roles = RoleName.Administrator + "," + RoleName.Editor)]
    public class ProductManageController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly ILogger<ProductManageController> _logger;

        public ProductManageController(AppDbContext context, UserManager<AppUser> userManager, ILogger<ProductManageController> logger)
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
            // var appDbContext = _context.Products.Include(p => p.Author);
            // return View(await appDbContext.ToListAsync());
            var posts = _context.Products
                        .Include(p => p.Author)
                        .OrderByDescending(p => p.DateUpdated);

            var totalPosts = await _context.Products.CountAsync();
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
                        .Include(p => p.productCategoryProducts)
                        .ThenInclude(pc => pc.Category)
                        .ToListAsync();
                        
            return View(postsInPage);
            // return View(await posts.ToListAsync());/

        }

        // GET: Post/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var post = await _context.Products
                .Include(p => p.Author)
                .FirstOrDefaultAsync(m => m.ProductId == id);
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
            var categories = await _context.CategoryProducts.ToListAsync();
            ViewData["categories"] = new MultiSelectList(categories, "Id", "Title");
            return View();
        }

        // POST: Post/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Description,Slug,Content,Published,DateCreated,DateUpdated,CategoryIDs,Price")] CreateProductModel product)
        {
            var categories = await _context.CategoryProducts.ToListAsync();
            ViewData["categories"] = new MultiSelectList(categories, "Id", "Title");
        
            if (product.Slug == null) {
                product.Slug = App.Utilities.AppUtilities.GenerateSlug(product.Title);
            }

            if (await _context.Products.AnyAsync(p => p.Slug == product.Slug)) {
                ModelState.AddModelError("Slug", "Nhập chuỗi URL khác");
                return View(product);
            }

            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(this.User);
                product.AuthorId = user.Id;

                _context.Add(product);
                if (product.CategoryIDs != null) {
                    foreach (var categoryid in product.CategoryIDs) {
                        _context.Add(new ProductCategoryProduct {
                            CategoryID = categoryid,
                            Product = product
                        });
                    }
                }

                await _context.SaveChangesAsync();
                StatusMessage = "Đã tạo sản phẩm mới";
                return RedirectToAction(nameof(Index)); 
            }

            return View(product);
        }

        // GET: Post/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // var post = await _context.Products.FindAsync(id);
            var product = await _context.Products.Include(p => p.productCategoryProducts).FirstOrDefaultAsync(p => p.ProductId == id);
            if (product == null)
            {
                return NotFound();
            }
            var postEdit = new CreateProductModel() {
                ProductId = product.ProductId,
                Title = product.Title,
                Content = product.Content,
                Description = product.Description,
                Slug = product.Slug,
                Published = product.Published,
                DateCreated = product.DateCreated,
                DateUpdated = product.DateUpdated,
                CategoryIDs = product.productCategoryProducts.Select(pc => pc.CategoryID).ToArray(),
                Price = product.Price
            };

            var categories = await _context.CategoryProducts.ToListAsync();
            ViewData["categories"] = new MultiSelectList(categories, "Id", "Title");
            // ViewData["AuthorId"] = new SelectList(_context.Users, "Id", "Id", post.AuthorId);
            return View(postEdit);
        }

        // POST: Post/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ProductId,Title,Description,Slug,Content,Published,DateCreated,DateUpdated,CategoryIDs,Price")] CreateProductModel product)
        {
            
            if (id != product.ProductId)
            {
                return NotFound();
            }

            var categories = await _context.CategoryProducts.ToListAsync();
            ViewData["categories"] = new MultiSelectList(categories, "Id", "Title");

            if (product.Slug == null) {
                product.Slug = App.Utilities.AppUtilities.GenerateSlug(product.Title);
            }

            if (await _context.Products.AnyAsync(p => p.Slug == product.Slug && p.ProductId != id)) {
                ModelState.AddModelError("Slug", "Nhập chuỗi URL khác");
                return View(product);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var productUpdate = await _context.Products.Include(p => p.productCategoryProducts).FirstOrDefaultAsync(p => p.ProductId == id);
                    if (productUpdate == null)
                        return NotFound();

                    productUpdate.Title = product.Title;
                    productUpdate.Description = product.Description;
                    productUpdate.Content = product.Content;
                    productUpdate.Published = product.Published;
                    productUpdate.Slug = product.Slug;
                    productUpdate.DateCreated = product.DateCreated;
                    productUpdate.DateUpdated = product.DateUpdated;
                    productUpdate.Price = product.Price;

                    // UPdate PostCategory
                    if (product.CategoryIDs == null)
                        product.CategoryIDs = new int[]{};
                    var oldCateIds = productUpdate.productCategoryProducts.Select(pc => pc.CategoryID).ToArray();
                    var newCateIds = product.CategoryIDs;

                    var removeCatePosts = from postCate in productUpdate.productCategoryProducts
                                            where(!newCateIds.Contains(postCate.CategoryID))
                                            select postCate;
                    _context.ProductCategoryProducts.RemoveRange(removeCatePosts);

                    var addCateIds = from Cateid in newCateIds
                                    where !oldCateIds.Contains(Cateid)
                                    select Cateid;

                    foreach (var CateId in addCateIds) {
                        _context.ProductCategoryProducts.Add(new ProductCategoryProduct {
                            ProductId = id,
                            CategoryID = CateId
                        });
                    }

                    
                    _context.Update(productUpdate);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PostExists(product.ProductId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                StatusMessage = "Đã cập nhật sản phẩm";
                return RedirectToAction(nameof(Index));
            }
            ViewData["AuthorId"] = new SelectList(_context.Users, "Id", "Id", product.AuthorId);
            return View(product);
        }

        // GET: Post/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Products == null)
            {
                return NotFound();
            }

            var post = await _context.Products
                .Include(p => p.Author)
                .FirstOrDefaultAsync(m => m.ProductId == id);
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
            if (_context.Products == null)
            {
                return Problem("Entity set 'AppDbContext.Posts'  is null.");
            }
            var post = await _context.Products.FindAsync(id);
            if (post != null)
            {
                _context.Products.Remove(post);
                StatusMessage = "Bạn đã xóa sản phẩm: " + post.Title;
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PostExists(int id)
        {
          return (_context.Products?.Any(e => e.ProductId == id)).GetValueOrDefault();
        }

        public class UploadOneFile {
            [Required]
            [DataType(DataType.Upload)]
            [FileExtensions(Extensions = "png, jpg, jpeg, gif")]
            [Display(Name = "Chọn file upload")]
            public IFormFile FileUpload {get; set;}
        }

        [HttpGet]
        public IActionResult UploadPhoto(int id) {
            var product = _context.Products.Where(p => p.ProductId == id)
            .Include(p => p.Photos)
            .FirstOrDefault();

            if (product == null)
                return NotFound("Không tìm thấy sản phẩm");

            ViewData["product"] = product;
            return View(new UploadOneFile());
        }

        [HttpPost, ActionName("UploadPhoto")]
        public async Task<IActionResult> UploadPhotoAsync(int id, [Bind("FileUpload")] UploadOneFile file) {
            var product = _context.Products.Where(p => p.ProductId == id)
            .Include(p => p.Photos)
            .FirstOrDefault();

            if (product == null)
                return NotFound("Không tìm thấy sản phẩm");

            ViewData["product"] = product;

            if (file != null) {
                var fileName = Path.GetFileNameWithoutExtension(Path.GetRandomFileName())
                                + Path.GetExtension(file.FileUpload.FileName);
                var fileUpload = Path.Combine("Uploads", "Products", fileName);

                using (var fileStream = new FileStream(fileUpload, FileMode.Create)) {
                    await file.FileUpload.CopyToAsync(fileStream);
                }
                
                _context.Add(new ProductPhoto() {
                    ProductId = product.ProductId,
                    FileName = fileName
                });

                await _context.SaveChangesAsync();
            }
            return View(file);
        }   
        [HttpPost]
        public IActionResult ListPhotos(int id) {
            var product = _context.Products.Where(p => p.ProductId == id)
                                            .Include(p => p.Photos)
                                            .FirstOrDefault();

            if (product == null) {
                return Json(new {
                    success = 0,
                    message = "Product not found"
                });
            }

            var listPhotos = product.Photos.Select(p => new {
                id = p.Id,
                path = "/contents/Products/" + p.FileName
            });

            return Json(new {
                success = 1,
                photos = listPhotos
            });
         }

        public IActionResult DeletePhoto(int id) {
            var photo = _context.ProductPhots.Where(p => p.Id == id).FirstOrDefault();
            if (photo != null) {
                _context.Remove(photo);
                _context.SaveChanges();

                var filename = "Uploads/Products/" + photo.FileName;
                System.IO.File.Delete(filename);
            }
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> UploadPhotoAPI(int id, [Bind("FileUpload")] UploadOneFile file) {
            var product = _context.Products.Where(p => p.ProductId == id)
                                            .Include(p => p.Photos)
                                            .FirstOrDefault();
            if (product == null)
                return NotFound("Không tìm thấy sản phẩm");
            if (file != null) {
                var fileName = Path.GetFileNameWithoutExtension(Path.GetRandomFileName())
                                + Path.GetExtension(file.FileUpload.FileName);
                var fileUpload = Path.Combine("Uploads", "Products", fileName);
                using (var fileStream = new FileStream(fileUpload, FileMode.Create)) {
                    await file.FileUpload.CopyToAsync(fileStream);
                }
                _context.Add(new ProductPhoto() {
                    ProductId = product.ProductId,
                    FileName = fileName
                });
                await _context.SaveChangesAsync();
            }
            return Ok();
        }
    }
}
