using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using FigureShop.Models;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using System.Linq;
using System;
using FigureShop.ViewModels;

namespace FigureShop.Controllers
{
    public class OrderController : Controller
    {
        // ...existing fields and constructor...
        // Xem lịch sử đơn hàng của user
        [Authorize]
        public async Task<IActionResult> OrderHistory()
        {
            var userId = _userManager.GetUserId(User);
            var orders = await _context.Orders.Include(o => o.OrderItems).ThenInclude(oi => oi.Product)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
            return View(orders);
        }
        private readonly FigureShopContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public OrderController(FigureShopContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        // Hiển thị danh sách đơn hàng cho admin
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var orders = await _context.Orders.Include(o => o.User)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
            return View(orders);
        }
        // Xóa đơn hàng (Admin)
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null) return NotFound();
            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
        // ...existing code...
        // ...existing code...

        // Chi tiết đơn hàng
        [Authorize(Roles = "Admin")]
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

        // GET: Order/Checkout (hiển thị tổng tiền giỏ hàng, chỉ các sản phẩm đã chọn)
        public IActionResult Checkout(string selectedItemIds)
        {
            var userId = _userManager.GetUserId(User);
            var cart = _context.ShoppingCarts.Include(c => c.Items).ThenInclude(i => i.Product).FirstOrDefault(c => c.UserId == userId);
            if (cart == null || !cart.Items.Any()) return RedirectToAction("Index", "ShoppingCart");
            List<ShoppingCartItem> selectedItems;
            if (!string.IsNullOrEmpty(selectedItemIds))
            {
                var ids = selectedItemIds.Split(',').Select(id => int.TryParse(id, out var i) ? i : -1).Where(i => i > 0).ToList();
                selectedItems = cart.Items.Where(i => ids.Contains(i.Id)).ToList();
            }
            else
            {
                selectedItems = cart.Items.ToList();
            }
            if (!selectedItems.Any()) return RedirectToAction("Index", "ShoppingCart");
            var vm = new CheckoutViewModel
            {
                TotalAmount = selectedItems.Sum(i => i.Quantity * i.Price),
                CartItems = selectedItems
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Checkout(CheckoutViewModel model, string selectedItemIds)
        {
            var userId = _userManager.GetUserId(User);
            var cart = _context.ShoppingCarts.Include(c => c.Items).FirstOrDefault(c => c.UserId == userId);
            if (cart == null || !cart.Items.Any()) return RedirectToAction("Index", "ShoppingCart");
            // Lấy danh sách sản phẩm đã chọn
            List<ShoppingCartItem> selectedItems;
            if (!string.IsNullOrEmpty(selectedItemIds))
            {
                var ids = selectedItemIds.Split(',').Select(id => int.TryParse(id, out var i) ? i : -1).Where(i => i > 0).ToList();
                selectedItems = cart.Items.Where(i => ids.Contains(i.Id)).ToList();
            }
            else
            {
                selectedItems = cart.Items.ToList();
            }
            if (!selectedItems.Any()) return RedirectToAction("Index", "ShoppingCart");
            decimal discount = 0;
            string voucherNote = null;
            string voucherCode = model.VoucherCode;
            // Xử lý các loại voucher
            if (!string.IsNullOrEmpty(voucherCode))
            {
                if (voucherCode == "NEW50")
                {
                    bool isNewUser = !_context.Orders.Any(o => o.UserId == userId);
                    bool hasFullInfo = !string.IsNullOrWhiteSpace(model.FullName) && !string.IsNullOrWhiteSpace(model.Address) && !string.IsNullOrWhiteSpace(model.Phone);
                    if (isNewUser && hasFullInfo)
                    {
                        discount = 50000;
                        voucherNote = "Đã áp dụng mã giảm 50,000₫ cho người mới.";
                    }
                }
                else if (voucherCode == "FREESHIP")
                {
                    discount = 30000; // Ví dụ miễn phí ship 30k
                    voucherNote = "Đã áp dụng mã miễn phí vận chuyển.";
                }
            }
            var order = new Order
            {
                UserId = userId,
                OrderDate = DateTime.Now,
                TotalAmount = selectedItems.Sum(i => i.Quantity * i.Price) - discount,
                Status = "Preparing",
                PaymentMethod = model.PaymentMethod,
                Address = model.Address,
                VoucherCode = voucherCode,
                VoucherDiscount = discount
            };
            if (voucherNote != null)
            {
                TempData["VoucherMessage"] = voucherNote;
            }
            _context.Orders.Add(order);
            _context.SaveChanges();
            foreach (var item in selectedItems)
            {
                var orderItem = new OrderItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    Price = item.Price,
                    OrderId = order.Id
                };
                _context.OrderItems.Add(orderItem);

                // Trừ tồn kho sản phẩm
                var product = _context.Products.FirstOrDefault(p => p.Id == item.ProductId);
                if (product != null)
                {
                    product.Stock -= item.Quantity;
                    if (product.Stock < 0) product.Stock = 0;
                    _context.Products.Update(product);
                }
            }
            _context.SaveChanges();
            // Chỉ xóa các sản phẩm đã chọn khỏi giỏ hàng
            _context.ShoppingCartItems.RemoveRange(selectedItems);
            _context.SaveChanges();
            return RedirectToAction("OrderSuccess", new { orderId = order.Id });
        }

        // Đổi trạng thái đơn hàng (Admin)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ChangeStatus(int orderId, string newStatus)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order != null)
            {
                order.Status = newStatus;
                _context.Update(order);
                await _context.SaveChangesAsync();
                string message;
                if (newStatus == "Shipped")
                    message = $"Đơn hàng đang được giao tới bạn! #{order.Id}";
                else if (newStatus == "Completed")
                    message = $"Đơn hàng đã đến chỗ bạn, vui lòng để ý điện thoại! #{order.Id}";
                else if (newStatus == "Cancelled")
                    message = $"Đơn hàng của bạn đã bị hủy! Xin lỗi bạn vì lý do bất tiện này. #{order.Id}";
                else
                    message = null;
                if (!string.IsNullOrEmpty(message))
                {
                    var notification = new Notification {
                        UserId = order.UserId,
                        Message = message,
                        CreatedAt = DateTime.Now,
                        IsRead = false,
                        OrderId = order.Id
                    };
                    _context.Notifications.Add(notification);
                    await _context.SaveChangesAsync();
                }
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: Order/OrderSuccess
        public async Task<IActionResult> OrderSuccess(int orderId)
        {
            var orderFull = await _context.Orders.Include(o => o.OrderItems).ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(m => m.Id == orderId);
            if (orderFull == null) return NotFound();
            // Tạo notification nếu đơn hàng tồn tại
            var notificationOrder = await _context.Orders.FindAsync(orderId);
            if (notificationOrder != null)
            {
                var notification = new Notification {
                    UserId = notificationOrder.UserId,
                    Message = $"Đơn hàng của bạn đã được xác nhận! #{notificationOrder.Id}",
                    CreatedAt = DateTime.Now,
                    IsRead = false,
                    OrderId = notificationOrder.Id
                };
                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();
            }
            return View(orderFull);
        }
    }
}
