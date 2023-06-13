B1
    ## Controller
    -   Là một lớp kế thừa từ lớp Controller: Microsoft.AspNetCore.Mvc.Controller
    -   Action trong Controller là một phương thức public (không được static)
    -   Action trả về bất kỳ kiểu dữ liệu nào, thường là IActionResult
    -   Các dịch vụ Inject vào Controller qua hàm tạo
    ## View
    -   Là file.cshtml
    -   View cho Action lưu tại: /View/ControllerName/ActionName.cshtml
    -   Thêm thư mục lưu trư View:
    ```
    //  {0} -> tên Action
    //  {1} -> tên Controller
    //  {2} -> tên Area
    options.PageViewLocationFormats.Add("/MyView/{1}/{0}" + RazorViewEngine.    ViewExtension)

    ## Truyền dữ liệu sang View
    -   Model
    -   ViewData
    -   ViewBag
    -   TempData

B2
    ## Areas
    -   Là tên dùng để routing
    -   Là cấu trúc thư mục M.V.C
    -   Thiết lập Area cho Controller bằng ```[Area("AreaName")]```
    -   Tạo cấu trúc thư mục
            ```
            dotnet aspnet-codegenerator area Product
            ```
    ## Route
    -   endpoints.MapControllerRoute
    -   endpoints.MapAreaControllerRoute
    -   [AcceptVerb("POST", "GET")]
    -   [Route("pattern")]
    -   [HttpGet] [HttpPost]
    ## Url Generation
        ### UrlHelper: Action, ActionLink, RouteUrl, Link
            ```
            Url.Action("PlanetInfo", "Planet", new {id = 1}, Context.   Request.   Scheme)
        
            Url.RouteUrl("default", new {controller = "First",      action="HellowView", id = 1, username="VNC"})
            ```
        ###HtmlTagHelper: ``` <a> <button> <form> ```
            Sử dụng thuộc tính:
            ```
            asp-area="Area"
            asp-action="Action"
            asp-controller="Product"
            asp-route...="123"
            asp-route="default"
            ```