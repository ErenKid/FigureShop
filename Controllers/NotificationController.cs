using FigureShop.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace FigureShop.Controllers
{
    [Authorize]
    public class NotificationController : Controller
    {
        private readonly FigureShopContext _context;
        public NotificationController(FigureShopContext context)
        {
            _context = context;
        }
        [HttpPost]
        public IActionResult Delete(int id)
        {
            var userName = User.Identity.Name;
            var user = _context.Users.FirstOrDefault(u => u.UserName == userName);
            if (user == null) return Unauthorized();
            var notification = _context.Notifications.FirstOrDefault(n => n.Id == id && n.UserId == user.Id && !n.IsRead);
            if (notification != null)
            {
                notification.IsRead = true;
                _context.Notifications.Update(notification);
                _context.SaveChanges();
                return Ok();
            }
            return NotFound();
        }
    }
}
