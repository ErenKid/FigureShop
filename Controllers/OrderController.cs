using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FigureShop.Models;
using System.Threading.Tasks;
using System.Linq;
using System;
using FigureShop.ViewModels;

namespace FigureShop.Controllers
{
    public class OrderController : Controller
    {
        private readonly FigureShopContext _context;

        public OrderController(FigureShopContext context)
        {
            _context = context;
        }

        // Hiển thị danh sách các đơn hàng
        public async Task<IActionResult> Index()
        {
            var orders = await _context.Orders.Include(o => o.OrderItems).ToListAsync();
            return View(orders);
        }

        // Chi tiết đơn hàng
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders.Include(o => o.OrderItems)
                                              .ThenInclude(oi => oi.Product)
                                              .FirstOrDefaultAsync(m => m.Id == id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // GET: Order/Checkout (hiển thị tổng tiền giỏ hàng)
        public IActionResult Checkout()
        {
            var userId = User.Identity?.Name ?? "guest";
            var cart = _context.ShoppingCarts.Include(c => c.Items).ThenInclude(i => i.Product).FirstOrDefault(c => c.UserId == userId);
            if (cart == null || !cart.Items.Any()) return RedirectToAction("Index", "ShoppingCart");
            var vm = new CheckoutViewModel
            {
                TotalAmount = cart.TotalAmount,
                CartItems = cart.Items
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Checkout(CheckoutViewModel model)
        {
            var userId = User.Identity?.Name ?? "guest";
            var cart = _context.ShoppingCarts.Include(c => c.Items).FirstOrDefault(c => c.UserId == userId);
            if (cart == null || !cart.Items.Any()) return RedirectToAction("Index", "ShoppingCart");
            var order = new Order
            {
                UserId = userId,
                OrderDate = DateTime.Now,
                TotalAmount = cart.TotalAmount,
                Status = "Preparing",
                PaymentMethod = model.PaymentMethod,
                // Nếu muốn lưu thông tin nhận hàng, có thể thêm property Note hoặc các property riêng
                // Note = $"Tên: {model.FullName}, Địa chỉ: {model.Address}, SĐT: {model.Phone}, Ghi chú: {model.Note}"
            };
            _context.Orders.Add(order);
            _context.SaveChanges();
            foreach (var item in cart.Items)
            {
                var orderItem = new OrderItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    Price = item.Price,
                    OrderId = order.Id
                };
                _context.OrderItems.Add(orderItem);
            }
            _context.SaveChanges();
            _context.ShoppingCartItems.RemoveRange(cart.Items);
            _context.SaveChanges();
            return RedirectToAction("OrderSuccess", new { orderId = order.Id });
        }

        // Đổi trạng thái đơn hàng (Admin)
        public async Task<IActionResult> ChangeStatus(int orderId, string newStatus)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order != null)
            {
                order.Status = newStatus;
                _context.Update(order);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: Order/OrderSuccess
        public async Task<IActionResult> OrderSuccess(int orderId)
        {
            var order = await _context.Orders.Include(o => o.OrderItems).ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(m => m.Id == orderId);
            if (order == null) return NotFound();
            return View(order);
        }
    }
}
