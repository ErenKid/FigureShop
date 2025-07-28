using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using FigureShop.Models;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using System.Linq;
using System;

namespace FigureShop.Controllers
{
    public class ProductReviewController : Controller
    {
        private readonly FigureShopContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProductReviewController(FigureShopContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> EditRating(int id, int productId, int rating)
        {
            var userId = _userManager.GetUserId(User);
            var review = await _context.ProductReviews.FindAsync(id);
            if (review == null || review.UserId != userId || review.ProductId != productId)
                return NotFound();
            review.Rating = rating;
            await _context.SaveChangesAsync();
            return RedirectToAction("Details", "Product", new { id = productId });
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Add(int productId, int rating)
        {
            var userId = _userManager.GetUserId(User);
            // Chỉ cho phép 1 review sao cho mỗi user mỗi sản phẩm
            var existing = _context.ProductReviews.FirstOrDefault(r => r.ProductId == productId && r.UserId == userId);
            if (existing != null)
                return RedirectToAction("Details", "Product", new { id = productId });
            var review = new ProductReview
            {
                ProductId = productId,
                UserId = userId,
                Rating = rating,
                Comment = "",
                CreatedAt = DateTime.Now
            };
            _context.ProductReviews.Add(review);
            await _context.SaveChangesAsync();
            return RedirectToAction("Details", "Product", new { id = productId });
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddComment(int productId, string comment)
        {
            var userId = _userManager.GetUserId(User);
            var review = _context.ProductReviews.FirstOrDefault(r => r.ProductId == productId && r.UserId == userId);
            if (review != null)
            {
                // Nếu đã có review thì chỉ cập nhật comment và thời gian
                review.Comment = comment;
                review.CreatedAt = DateTime.Now;
            }
            else
            {
                // Nếu chưa có thì tạo mới
                review = new ProductReview
                {
                    ProductId = productId,
                    UserId = userId,
                    Rating = 0,
                    Comment = comment,
                    CreatedAt = DateTime.Now
                };
                _context.ProductReviews.Add(review);
            }
            await _context.SaveChangesAsync();
            return RedirectToAction("Details", "Product", new { id = productId });
        }

        [Authorize]
        public async Task<IActionResult> EditComment(int id)
        {
            var userId = _userManager.GetUserId(User);
            var review = await _context.ProductReviews.FindAsync(id);
            if (review == null || review.UserId != userId)
                return NotFound();
            // Trả về lại trang chi tiết sản phẩm với thông tin để hiện form sửa tại chỗ
            return RedirectToAction("Details", "Product", new { id = review.ProductId, editingCommentId = review.Id, editingCommentValue = review.Comment });
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> EditComment(int id, string comment)
        {
            var userId = _userManager.GetUserId(User);
            var review = await _context.ProductReviews.FindAsync(id);
            if (review == null || review.UserId != userId)
                return NotFound();
            review.Comment = comment;
            await _context.SaveChangesAsync();
            return RedirectToAction("Details", "Product", new { id = review.ProductId });
        }

        [Authorize]
        public async Task<IActionResult> DeleteComment(int id)
        {
            var userId = _userManager.GetUserId(User);
            var review = await _context.ProductReviews.FindAsync(id);
            if (review == null || review.UserId != userId)
                return NotFound();
            var productId = review.ProductId;
            _context.ProductReviews.Remove(review);
            await _context.SaveChangesAsync();
            return RedirectToAction("Details", "Product", new { id = productId });
        }
    }
}
