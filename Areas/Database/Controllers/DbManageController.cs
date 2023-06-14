using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using App.Data;
using App.Models;
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
            StatusMessage = "Đã Seed dữ liệu vào DB";
            return RedirectToAction("Index");
         }


    }
}