using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using App.Models;
using App.Models.Product;
using AppMvc.Areas.Product.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AppMvc.Net.Areas.Product.Controllers
{
    [Area("Product")]
    public class ViewProductController : Controller
    {
        private readonly ILogger<ViewProductController> _logger;
        private readonly AppDbContext _context;
        private readonly CartService _cartService;

        public ViewProductController(ILogger<ViewProductController> logger, AppDbContext context, CartService cartService)
        {
            _logger = logger;
            _context = context;
            _cartService = cartService;
        }
        [Route("/product/{categoryslug?}")]
        public IActionResult Index(string categoryslug,[FromQuery(Name = "p")]int currentPage, int pagesize)
        {
            var categories = GetCategories();
            ViewBag.categories = categories;
            ViewBag.categoryslug = categoryslug;

            CategoryProduct? category = null;
            if (!string.IsNullOrEmpty(categoryslug)) {
                category = _context.CategoryProducts.Where(c => c.Slug == categoryslug)
                                                .Include(c => c.CategoryChildren)
                                                .FirstOrDefault();
                if (category == null)
                    return NotFound("Không tìm thấy Category");
            }

            var products = _context.Products
                        .Include(p => p.Author)
                        .Include(p => p.Photos)
                        .Include(p => p.productCategoryProducts)
                        .ThenInclude(p => p.Category)
                        .AsQueryable();

            products = products.OrderByDescending(p => p.DateUpdated);

            if (category != null) {
                var ids = new List<int>();
                category.ChildCategoryIDs(ids, null);
                ids.Add(category.Id);

                products = products.Where(p => p.productCategoryProducts.Where(pc => ids.Contains(pc.CategoryID)).Any());
            }

            var totalProducts = products.Count();
            if (pagesize <= 0)
                pagesize = 6;
            int countPages = (int)Math.Ceiling((double)totalProducts / pagesize);
            
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
            var productsInPage = products.Skip((currentPage - 1) * pagesize)
                        .Take(pagesize);

            ViewBag.PagingModel = pagingModel;
            ViewBag.totalProducts = totalProducts;

            ViewBag.category = category;
            return View(productsInPage.ToList());
        }

        
        [Route("/product/{productslug}.html")]
        public IActionResult Details(string productslug) {
            var categories = GetCategories();
            ViewBag.categories = categories;

            var products = _context.Products.Where(p => p.Slug == productslug)
                                        .Include(p => p.Author)
                                        .Include(p => p.Photos)
                                        .Include(p => p.productCategoryProducts)
                                        .ThenInclude(p => p.Category)
                                        .FirstOrDefault();
            if (products == null)
                return NotFound("Khong tim thay");

            CategoryProduct? category = products.productCategoryProducts?.FirstOrDefault()?.Category;
            ViewBag.category = category;

            var otherProducts = _context.Products.Where(p => p.productCategoryProducts.Any(c => c.Category.Id == category.Id))
                                            .Where(p => p.ProductId != products.ProductId)
                                            .OrderByDescending(p => p.DateUpdated)
                                            .Take(5);
            ViewBag.otherProducts = otherProducts;

            return View(products);
        }

        private List<CategoryProduct> GetCategories() {
            var categories = _context.CategoryProducts
                            .Include(c => c.CategoryChildren)
                            .AsEnumerable()
                            .Where(c => c.ParentCategory == null)
                            .ToList();
            return categories;
        }  

        /// Thêm sản phẩm vào cart
        [Route ("addcart/{productid:int}", Name = "addcart")]
        public IActionResult AddToCart ([FromRoute] int productid) {

            var product = _context.Products
                .Where (p => p.ProductId == productid)
                .FirstOrDefault ();
            if (product == null)
                return NotFound ("Không có sản phẩm");

            // Xử lý đưa vào Cart ...
            var cart = _cartService.GetCartItems ();
            var cartitem = cart.Find (p => p.product.ProductId == productid);
            if (cartitem != null) {
                // Đã tồn tại, tăng thêm 1
                cartitem.quantity++;
            } else {
                //  Thêm mới
                cart.Add (new CartItem () { quantity = 1, product = product });
            }

            // Lưu cart vào Session
            _cartService.SaveCartSession (cart);
            // Chuyển đến trang hiện thị Cart
            return RedirectToAction (nameof (Cart));
        }

        // Hiện thị giỏ hàng
        [Route ("/cart", Name = "cart")]
        public IActionResult Cart () {
            return View (_cartService.GetCartItems());
        }

        /// xóa item trong cart
        [Route ("/removecart/{productid:int}", Name = "removecart")]
        public IActionResult RemoveCart ([FromRoute] int productid) {
            var cart = _cartService.GetCartItems ();
            var cartitem = cart.Find (p => p.product.ProductId == productid);
            if (cartitem != null) {
                // Đã tồn tại, tăng thêm 1
                cart.Remove(cartitem);
            }

            _cartService.SaveCartSession (cart);
            return RedirectToAction (nameof (Cart));
        }

        /// Cập nhật
        [Route ("/updatecart", Name = "updatecart")]
        [HttpPost]
        public IActionResult UpdateCart ([FromForm] int productid, [FromForm] int quantity) {
            // Cập nhật Cart thay đổi số lượng quantity ...
            var cart = _cartService.GetCartItems ();
            var cartitem = cart.Find (p => p.product.ProductId == productid);
            if (cartitem != null) {
                // Đã tồn tại, tăng thêm 1
                cartitem.quantity = quantity;
            }
            _cartService.SaveCartSession (cart);
            // Trả về mã thành công (không có nội dung gì - chỉ để Ajax gọi)
            return Ok();
        }

        [Route ("/checkout")]
        public IActionResult Checkout() {
            var cart = _cartService.GetCartItems ();

            // ...
            _cartService.ClearCart();
            return Content("Đã gửi đơn hàng");
        }

    }
}