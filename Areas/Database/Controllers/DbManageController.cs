using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using App.Data;
using App.Models;
using App.Models.Blog;
using App.Models.Product;
using Bogus;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace App.Areas.Database.Controllers
{
    [Area("Database")]
    [Route("/database-manage/[action]")]
    public class DbManageController : Controller
    {
        private readonly UserManager<AppUser> _userManage;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManage;
        private readonly AppDbContext _dbContext;

        public DbManageController(UserManager<AppUser> userManage, RoleManager<IdentityRole> roleManage, AppDbContext dbContext, SignInManager<AppUser> signInManager)
        {
            _userManage = userManage;
            _roleManage = roleManage;
            _dbContext = dbContext;
            _signInManager = signInManager;
        }

        [TempData]
        public string StatusMessage { get; set; }
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [Authorize(Roles = RoleName.Administrator)]
        public IActionResult DeleteDb() {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = RoleName.Administrator)]
        public async Task<IActionResult> DeleteDbAsync() {
            var result = await _dbContext.Database.EnsureDeletedAsync();
            StatusMessage = result ? "Đã xóa DB" : "Không thể xóa Db";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> MigrationAsync() {
            await _dbContext.Database.MigrateAsync();
            StatusMessage = "Đã tạo Db";
            return RedirectToAction(nameof(Index));
        }

         public async Task<IActionResult> SeedDataAsync(){
            // Create Roles
            var rolenames = typeof(RoleName).GetFields().ToList();
            foreach (var r in rolenames) {
                var rolename = (string)r.GetRawConstantValue();
                var rfound = await _roleManage.FindByNameAsync(rolename);
                if (rfound == null)
                    await _roleManage.CreateAsync(new IdentityRole(rolename));
            }
            // Tao admin , pwd = admin123, admin@example.com
            var useradmin = await _userManage.FindByNameAsync("admin");
            if (useradmin == null) {
                useradmin = new AppUser {
                    UserName = "admin",
                    Email = "admin@example.com",
                    EmailConfirmed = true
                };
                await _userManage.CreateAsync(useradmin, "qweqwe");
                await _userManage.AddToRoleAsync(useradmin, RoleName.Administrator);
                await _signInManager.SignInAsync(useradmin, false);

                return RedirectToAction(nameof(SeedDataAsync));
            } else {
                var user = await _userManage.GetUserAsync(this.User);
                if (user == null)
                    return this.Forbid();
                var roles = await _userManage.GetRolesAsync(user);
                if (!roles.Any(r => r == RoleName.Administrator))
                    return this.Forbid();
            }
            
            SeedPostCategory();
            SeedProductCategory();

            StatusMessage = "Đã Seed dữ liệu vào DB";
            return RedirectToAction("Index");
         }

         private void SeedPostCategory() {
            _dbContext.Categories.RemoveRange(_dbContext.Categories.Where(c => c.Description.Contains("fakeData")));
            _dbContext.Posts.RemoveRange(_dbContext.Posts.Where(p => p.Content.Contains("fakeData")));
            
            _dbContext.SaveChanges();
            
            var fakerCategory = new Faker<Category>();
            int cm = 1;
            fakerCategory.RuleFor(c => c.Title, fk => $"CM{cm++} " + fk.Lorem.Sentence(1,2).Trim('.'));
            fakerCategory.RuleFor(c => c.Description, fk => fk.Lorem.Sentence(5) + "[fakeData]");
            fakerCategory.RuleFor(c => c.Slug, fk => fk.Lorem.Slug());

            var cate1 = fakerCategory.Generate();
            var cate11 = fakerCategory.Generate();
            var cate12 = fakerCategory.Generate();
            var cate2 = fakerCategory.Generate();
            var cate21 = fakerCategory.Generate();
            var cate211 = fakerCategory.Generate();

            cate11.ParentCategory = cate1;
            cate12.ParentCategory = cate1;
            cate21.ParentCategory = cate2;
            cate211.ParentCategory = cate21;

            var categories = new Category[] {cate1, cate11, cate12, cate2, cate21, cate211};
            _dbContext.Categories.AddRange(categories);


            // Fake Post
            var rCateIndex = new Random();
            int bv = 1;
            var user = _userManage.GetUserAsync(this.User).Result;

            var fakerProduct = new Faker<Post>();
            fakerProduct.RuleFor(p => p.AuthorId, f => user.Id);
            fakerProduct.RuleFor(p => p.Content, f => f.Lorem.Paragraphs(7) + "[fakeData]");
            fakerProduct.RuleFor(p => p.DateCreated, f => f.Date.Between(new DateTime(2021,1,1), new DateTime(2021,12,31)));
            fakerProduct.RuleFor(p => p.Description, f => f.Lorem.Sentences(3));
            fakerProduct.RuleFor(p => p.Published, f => true);
            fakerProduct.RuleFor(p => p.Slug, f => f.Lorem.Slug());
            fakerProduct.RuleFor(p => p.Title, f => $"Bài {bv++} " + f.Lorem.Sentence(3,4).Trim(','));

            List<Post> posts = new List<Post>();
            List<PostCategory> postCategories = new List<PostCategory>();

            for (int i = 0; i < 50; i++) {
                var post = fakerProduct.Generate();
                post.DateUpdated = post.DateCreated;
                posts.Add(post);
                postCategories.Add(new PostCategory {
                    Post = post,
                    Category = categories[rCateIndex.Next(5)]
                });
            }

            _dbContext.Posts.AddRange(posts);
            _dbContext.PostCategories.AddRange(postCategories);

            _dbContext.SaveChanges();
         }



         private void SeedProductCategory() {
            _dbContext.CategoryProducts.RemoveRange(_dbContext.CategoryProducts.Where(c => c.Description.Contains("fakeData")));
            _dbContext.Products.RemoveRange(_dbContext.Products.Where(p => p.Content.Contains("fakeData")));
            
            _dbContext.SaveChanges();
            
            var fakerCategory = new Faker<CategoryProduct>();
            int cm = 1;
            fakerCategory.RuleFor(c => c.Title, fk => $"Nhom SP{cm++} " + fk.Lorem.Sentence(1,2).Trim('.'));
            fakerCategory.RuleFor(c => c.Description, fk => fk.Lorem.Sentence(5) + "[fakeData]");
            fakerCategory.RuleFor(c => c.Slug, fk => fk.Lorem.Slug());

            var cate1 = fakerCategory.Generate();
            var cate11 = fakerCategory.Generate();
            var cate12 = fakerCategory.Generate();
            var cate2 = fakerCategory.Generate();
            var cate21 = fakerCategory.Generate();
            var cate211 = fakerCategory.Generate();

            cate11.ParentCategory = cate1;
            cate12.ParentCategory = cate1;
            cate21.ParentCategory = cate2;
            cate211.ParentCategory = cate21;

            var categories = new CategoryProduct[] {cate1, cate11, cate12, cate2, cate21, cate211};
            _dbContext.CategoryProducts.AddRange(categories);


            // Fake Post
            var rCateIndex = new Random();
            int bv = 1;
            var user = _userManage.GetUserAsync(this.User).Result;

            var fakerProduct = new Faker<ProductModel>();
            fakerProduct.RuleFor(p => p.AuthorId, f => user.Id);
            fakerProduct.RuleFor(p => p.Content, f => f.Commerce.ProductDescription() + "[fakeData]");
            fakerProduct.RuleFor(p => p.DateCreated, f => f.Date.Between(new DateTime(2021,1,1), new DateTime(2021,12,31)));
            fakerProduct.RuleFor(p => p.Description, f => f.Lorem.Sentences(3));
            fakerProduct.RuleFor(p => p.Published, f => true);
            fakerProduct.RuleFor(p => p.Slug, f => f.Lorem.Slug());
            fakerProduct.RuleFor(p => p.Title, f => $"SP {bv++} " + f.Commerce.ProductName());
            fakerProduct.RuleFor(p => p.Price, f => int.Parse(f.Commerce.Price(500, 1000, 0)));

            List<ProductModel> products = new List<ProductModel>();
            List<ProductCategoryProduct> productCategories = new List<ProductCategoryProduct>();

            for (int i = 0; i < 50; i++) {
                var product = fakerProduct.Generate();
                product.DateUpdated = product.DateCreated;
                products.Add(product);
                productCategories.Add(new ProductCategoryProduct {
                    Product = product,
                    Category = categories[rCateIndex.Next(5)]
                });
            }

            _dbContext.AddRange(products);
            _dbContext.AddRange(productCategories);

            _dbContext.SaveChanges();
         }



    }
}
                