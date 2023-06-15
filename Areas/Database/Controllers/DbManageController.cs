using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using App.Data;
using App.Models;
using App.Models.Blog;
using Bogus;
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
        private readonly RoleManager<IdentityRole> _roleManage;
        private readonly AppDbContext _dbContext;

        public DbManageController(UserManager<AppUser> userManage, RoleManager<IdentityRole> roleManage, AppDbContext dbContext)
        {
            _userManage = userManage;
            _roleManage = roleManage;
            _dbContext = dbContext;
        }

        [TempData]
        public string StatusMessage { get; set; }
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult DeleteDb() {
            return View();
        }

        [HttpPost]
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
            }
            
            SeedPostCategory();

            StatusMessage = "Đã Seed dữ liệu vào DB";
            return RedirectToAction("Index");
         }

         private void SeedPostCategory() {
            _dbContext.Categories.RemoveRange(_dbContext.Categories.Where(c => c.Description.Contains("fakeData")));
            _dbContext.Posts.RemoveRange(_dbContext.Posts.Where(p => p.Content.Contains("fakeData")));
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

            var fakerPost = new Faker<Post>();
            fakerPost.RuleFor(p => p.AuthorId, f => user.Id);
            fakerPost.RuleFor(p => p.Content, f => f.Lorem.Paragraphs(7) + "[fakeData]");
            fakerPost.RuleFor(p => p.DateCreated, f => f.Date.Between(new DateTime(2021,1,1), new DateTime(2021,12,31)));
            fakerPost.RuleFor(p => p.Description, f => f.Lorem.Sentences(3));
            fakerPost.RuleFor(p => p.Published, f => true);
            fakerPost.RuleFor(p => p.Slug, f => f.Lorem.Slug());
            fakerPost.RuleFor(p => p.Title, f => $"Bài {bv++} " + f.Lorem.Sentence(3,4).Trim(','));

            List<Post> posts = new List<Post>();
            List<PostCategory> postCategories = new List<PostCategory>();

            for (int i = 0; i < 50; i++) {
                var post = fakerPost.Generate();
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


    }
}
                