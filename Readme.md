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