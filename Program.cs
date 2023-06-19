using App.Data;
using App.Menu;
using App.Models;
using App.Services;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddDbContext<AppDbContext>(options =>
        {
            // builder.Configuration.GetConnectionString("ConnectionStrings:AppMvcConnectionString");
            string? connectString = builder.Configuration["ConnectionStrings:AppMvcConnectionString"];
            options.UseSqlServer(connectString);

        });

        builder.Services.AddOptions();
        // dùng Configuration để đọc MailSettings từ file config
        // đăng ký lớp cấu hình MailSettings được thiết lập dữ liệu từ MailSettings đọc được
        // và được Inject vào SendMailService
        builder.Services.Configure<MailSettings>(builder.Configuration.GetSection("MailSettings"));

        // Đăng ký dịch vụ SendMailService
        builder.Services.AddSingleton<IEmailSender, SendMailService>();

        builder.Services.AddSingleton<IdentityErrorDescriber, AppIdentityErrorDescriber>();

        // Add services to the container.
        builder.Services.AddControllersWithViews();
        builder.Services.AddRazorPages();       // them razor page

        builder.Services.AddSingleton<PlanetService>();
        // builder.Services.AddSingleton<ProductService>();

        builder.Services.AddTransient<CartService>();

        builder.Services.AddTransient<IActionContextAccessor, ActionContextAccessor>();

        builder.Services.AddTransient<AdminSidebarService>();

        // Dang ky Identity
        builder.Services.AddIdentity<AppUser, IdentityRole>()
                        .AddEntityFrameworkStores<AppDbContext>()
                        .AddDefaultTokenProviders();



        // Truy cập IdentityOptions
        builder.Services.Configure<IdentityOptions>(options =>
        {
            // Thiết lập về Password
            options.Password.RequireDigit = false; // Không bắt phải có số
            options.Password.RequireLowercase = false; // Không bắt phải có chữ thường
            options.Password.RequireNonAlphanumeric = false; // Không bắt ký tự đặc biệt
            options.Password.RequireUppercase = false; // Không bắt buộc chữ in
            options.Password.RequiredLength = 3; // Số ký tự tối thiểu của password
            options.Password.RequiredUniqueChars = 1; // Số ký tự riêng biệt

            // Cấu hình Lockout - khóa user
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5); // Khóa 5 phút
            options.Lockout.MaxFailedAccessAttempts = 3; // Thất bại 3 lần thì khóa
            options.Lockout.AllowedForNewUsers = true;

            // Cấu hình về User.
            options.User.AllowedUserNameCharacters = // các ký tự đặt tên user
                "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
            options.User.RequireUniqueEmail = true;  // Email là duy nhất

            // Cấu hình đăng nhập.
            options.SignIn.RequireConfirmedEmail = true;            // Cấu hình xác thực địa chỉ email (email phải tồn tại)
            options.SignIn.RequireConfirmedPhoneNumber = false;     // Xác thực số điện thoại
            options.SignIn.RequireConfirmedAccount = true;
        });
        // Authentication: xác thực danh tính -> Login, Logout
        builder.Services.ConfigureApplicationCookie(options =>
        {
            options.LoginPath = "/login/";
            options.LogoutPath = "/logout/";
            options.AccessDeniedPath = "/khongduoctruycap.html";
        });

        builder.Services.AddAuthentication()
                        .AddGoogle(googleOptions =>
                        {
                            var googleConfig = builder.Configuration.GetSection("web:Google");
                            googleOptions.ClientId = googleConfig["client_id"];
                            googleOptions.ClientSecret = googleConfig["client_secret"];
                            // https://localhost:7237/signin-google
                            googleOptions.CallbackPath = "/dang-nhap-tu-google";
                        })
                        .AddFacebook(facebookOptions =>
                        {
                            var facebookConfig = builder.Configuration.GetSection("web:Facebook");
                            facebookOptions.AppId = facebookConfig["app_id"];
                            facebookOptions.AppSecret = facebookConfig["app_secret"];
                            facebookOptions.CallbackPath = "/dang-nhap-tu-facebook";
                        });


        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy("ViewManageMenu", builder =>
            {
                builder.RequireAuthenticatedUser();
                builder.RequireRole(RoleName.Administrator);
            });
        });


        builder.Services.AddDistributedMemoryCache();   // Đăng ký dịch vụ lưu cache trong bộ nhớ (Session sẽ sử dụng nó)
        builder.Services.AddSession(cfg =>
        {             // Đăng ký dịch vụ Session
            cfg.Cookie.Name = "appmvc";                 // Đặt tên Session - tên này sử dụng ở Browser (Cookie)
            cfg.IdleTimeout = new TimeSpan(0, 30, 0);   // Thời gian tồn tại của Session
        });


        builder.Services.Configure<RazorViewEngineOptions>(options =>
        {
            //đường dẫn mặc định: /View/Controller/Action.cshtml
            // Thêm đường dẫn để tìm file cshtml để show ra View với RazorPage
            options.PageViewLocationFormats.Add("/MyView/{1}/{0}" + RazorViewEngine.ViewExtension);
        });

        // builder.WebHost.CreateDefaultBuilder(args)
        //                 .UseUrls("http://*:5000;http://localhost:5001;https://hostname:5002");

        builder.WebHost.UseUrls("http://0.0.0.0:8090");
            
        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseStaticFiles(new StaticFileOptions()
        {
            FileProvider = new PhysicalFileProvider(
                Path.Combine(Directory.GetCurrentDirectory(), "Uploads")
                ),
            RequestPath = "/contents"
        });

        app.UseSession();

        app.UseRouting();

        app.UseAuthentication();    // xac dinh danh tinh
        app.UseAuthorization();     // xac dinh quyen truy cap

        // app.MapControllerRoute(
        //     name: "default",
        //     pattern: "{controller=Home}/{action=Index}/{id?}");


        // MapController
        // MapControllerRoute
        // MapDefaultControlerRoute
        // MapAreaControllerRoute

        // URL: start-here/ten controller/ten action/id <=> start-here/Home/Index
        // app.MapControllerRoute(
        //     name: "firstrout",
        //     pattern: "start-here/{controller=Home}/{action=Index}/{id?}");


        // Areas/AreaName/Views/ControllerName/Action.html
        app.MapAreaControllerRoute(
            name: "default",
            pattern: "{controller}/{action}/{id?}",
            areaName: "Identity"
        );
        app.MapAreaControllerRoute(
            name: "default",
            pattern: "{controller}/{action=Index}/{id?}",
            areaName: "ProductManage"
        );

        // Controller khong co Area
        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");


        // app.MapControllerRoute(
        //     name: "firstrout",
        //     pattern: "{url:xemsanpham}/id:range(2,10)",
        //     defaults: new {
        //         controller = "Home",
        //         action = "Nothing"}
        //     // ,constraints: new {
        //     //     url = "xemsanpham",
        //     //     id = new RangeRouteConstraint(2, 10)}
        //     );


        app.UseEndpoints(options =>
        {
            options.MapGet("/sayhi", async (context) =>
            {
                await context.Response.WriteAsync($"Hello ASP.NET MVC {DateTime.Now}");
            });
            // options.MapRazorPages();
        });


        app.MapRazorPages();    // truy cap razor page


        app.Run();
    }

    public static IWebHostBuilder CreateWebHostBuilder(string[] args) {

        return WebHost.CreateDefaultBuilder(args)
            .UseUrls("http://*:5000")
            .Configure(app =>
            {
                app.Run(context => 
                    context.Response.WriteAsync("Hello, World!"));
            });
    }
}