using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FigureShop.Models;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using FigureShop.ViewModels;

namespace FigureShop.Controllers
{
    public class ShoppingCartController : Controller
    {
        private readonly FigureShopContext _context;

        public ShoppingCartController(FigureShopContext context)
        {
            _context = context;
        }

        // Thêm sản phẩm vào giỏ
        [HttpPost]
public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
{
    var cart = GetOrCreateCart();
    var product = await _context.Products.FindAsync(productId);
    if (product != null)
    {
        var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);
        if (item != null)
        {
            item.Quantity += quantity;
        }
        else
        {
            decimal finalPrice = product.DiscountPrice.HasValue && product.DiscountPrice > 0
                ? product.DiscountPrice.Value
                : product.Price;

            cart.Items.Add(new ShoppingCartItem
            {
                ProductId = productId,
                Quantity = quantity,
                Price = finalPrice,
                ShoppingCartId = cart.Id
            });
        }

        cart.TotalAmount = cart.Items.Sum(i => i.Quantity * i.Price);
        _context.SaveChanges();
    }
    return RedirectToAction("Index");
}


        // Xem giỏ hàng
        public IActionResult Index()
        {
            var cart = GetOrCreateCart();
            // Nạp thông tin sản phẩm cho từng item
            foreach (var item in cart.Items)
            {
                if (item.Product == null)
                {
                    item.Product = _context.Products.FirstOrDefault(p => p.Id == item.ProductId);
                }
            }
            return View(cart);
        }

        // Cập nhật số lượng sản phẩm trong giỏ
        [HttpPost]
        public IActionResult UpdateQuantity(int itemId, int quantity)
        {
            var cart = GetOrCreateCart();
            var item = cart.Items.FirstOrDefault(i => i.Id == itemId);
            if (item != null && quantity > 0)
            {
                item.Quantity = quantity;
                cart.TotalAmount = cart.Items.Sum(i => i.Quantity * i.Price);
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        // Xoá sản phẩm khỏi giỏ
        [HttpPost]
        public IActionResult RemoveItem(int itemId)
        {
            var cart = GetOrCreateCart();
            var item = cart.Items.FirstOrDefault(i => i.Id == itemId);
            if (item != null)
            {
                cart.Items.Remove(item);
                cart.TotalAmount = cart.Items.Sum(i => i.Quantity * i.Price);
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        // Thanh toán (giả lập)
        public IActionResult Checkout()
        {
            var cart = GetOrCreateCart();
            // Xử lý thanh toán giả lập: Xoá sạch giỏ hàng
            cart.Items.Clear();
            cart.TotalAmount = 0;
            _context.SaveChanges();
            ViewBag.Message = "Thanh toán thành công!";
            return View("Index", cart);
        }

        [HttpPost]
        public IActionResult AddMultipleToCart(List<SelectedProductViewModel> selectedProducts)
        {
            var cart = GetOrCreateCart();

            foreach (var item in selectedProducts)
            {
                if (item.IsSelected && item.Quantity > 0)
                {
                    var product = _context.Products.Find(item.ProductId);
                    if (product != null)
                    {
                        var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == item.ProductId);
                        if (existingItem != null)
                            existingItem.Quantity += item.Quantity;
                        else
                            cart.Items.Add(new ShoppingCartItem
                            {
                                ProductId = item.ProductId,
                                Quantity = item.Quantity,
                                Price = product.Price
                            });
                    }
                }
            }
            // TÍNH LẠI TỔNG TIỀN SAU KHI THÊM
            cart.TotalAmount = cart.Items.Sum(i => i.Quantity * i.Price);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        [HttpPost]
        public JsonResult UpdateQuantityAjax([FromBody] UpdateQuantityModel model)
        {
            var cart = GetOrCreateCart();
            var item = cart.Items.FirstOrDefault(i => i.Id == model.itemId);
            if (item != null && model.quantity > 0)
            {
                item.Quantity = model.quantity;
                cart.TotalAmount = cart.Items.Sum(i => i.Quantity * i.Price);
                _context.SaveChanges();
            }
            return Json(new {
                totalAmount = cart.TotalAmount.ToString("N0"),
                itemTotal = (item != null ? (item.Quantity * item.Price).ToString("N0") : "0")
            });
        }
        public class UpdateQuantityModel
        {
            public int itemId { get; set; }
            public int quantity { get; set; }
        }

        // Lấy hoặc tạo giỏ hàng cho user hiện tại
        private ShoppingCart GetOrCreateCart()
        {
            var userId = User.Identity?.Name ?? "guest";
            var cart = _context.ShoppingCarts
                .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefault(c => c.UserId == userId);
            if (cart == null)
            {
                cart = new ShoppingCart { UserId = userId, Items = new List<ShoppingCartItem>(), TotalAmount = 0 };
                _context.ShoppingCarts.Add(cart);
                _context.SaveChanges();
            }
            return cart;
        }
    }
} 