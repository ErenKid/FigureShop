using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using FigureShop.Models;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using System.Linq;
using System;

namespace FigureShop.Controllers
{
    public class ProductCommentController : Controller
    {
        private readonly FigureShopContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProductCommentController(FigureShopContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Add(int productId, string content)
        {
            var userId = _userManager.GetUserId(User);
            var user = await _userManager.FindByIdAsync(userId);
            var comment = new ProductComment
            {
                ProductId = productId,
                UserId = userId,
                Content = content,
                CreatedAt = DateTime.Now,
                User = user
            };
            _context.ProductComments.Add(comment);
            await _context.SaveChangesAsync();
            return RedirectToAction("Details", "Product", new { id = productId });
        }

        [Authorize]
        public async Task<IActionResult> Edit(int id)
        {
            var userId = _userManager.GetUserId(User);
            var comment = await _context.ProductComments.FindAsync(id);
            if (comment == null || comment.UserId != userId)
                return NotFound();
            return RedirectToAction("Details", "Product", new { id = comment.ProductId, editingCommentId = comment.Id, editingCommentValue = comment.Content });
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Edit(int id, string content)
        {
            var userId = _userManager.GetUserId(User);
            var comment = await _context.ProductComments.FindAsync(id);
            if (comment == null || comment.UserId != userId)
                return NotFound();
            comment.Content = content;
            await _context.SaveChangesAsync();
            return RedirectToAction("Details", "Product", new { id = comment.ProductId });
        }

        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = _userManager.GetUserId(User);
            var comment = await _context.ProductComments.FindAsync(id);
            if (comment == null || (comment.UserId != userId && !User.IsInRole("Admin")))
                return NotFound();
            var productId = comment.ProductId;
            _context.ProductComments.Remove(comment);
            await _context.SaveChangesAsync();
            return RedirectToAction("Details", "Product", new { id = productId });
        }
    }
}
