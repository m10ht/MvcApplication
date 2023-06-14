using App.Models;
using App.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options => {
    // builder.Configuration.GetConnectionString("ConnectionStrings:AppMvcConnectionString");
    string? connectString = builder.Configuration["ConnectionStrings:AppMvcConnectionString"];
    options.UseSqlServer(connectString);
    
});

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();       // them razor page

builder.Services.AddSingleton<PlanetService>();
builder.Services.AddSingleton<ProductService>();

builder.Services.Configure<RazorViewEngineOptions>(options => {
    //đường dẫn mặc định: /View/Controller/Action.cshtml
    // Thêm đường dẫn để tìm file cshtml để show ra View với RazorPage
    options.PageViewLocationFormats.Add("/MyView/{1}/{0}" + RazorViewEngine.ViewExtension);
});

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


app.UseEndpoints(options => {
    options.MapGet("/sayhi", async (context) => {
        await context.Response.WriteAsync($"Hello ASP.NET MVC {DateTime.Now}");
        });
    // options.MapRazorPages();
});


app.MapRazorPages();    // truy cap razor page

app.Run();
