using FigureShop.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using FigureShop.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace FigureShop.Controllers
{
    public class HomeController : Controller
    {
        private readonly FigureShopContext _context;
        private readonly ILogger<HomeController> _logger;

        public HomeController(FigureShopContext context, ILogger<HomeController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public IActionResult Index()
        {
            var categories = _context.Categories?.ToList() ?? new List<Category>();
            var now = DateTime.Now;
            string search = Request.Query["search"];
            var productsQuery = _context.Products
                .Include(p => p.Category)
                .Include(p => p.FlashSale)
                .OrderByDescending(p => p.Id);
            if (!string.IsNullOrEmpty(search))
            {
                productsQuery = productsQuery.Where(p => p.Name.Contains(search)).OrderByDescending(p => p.Id);
            }
            var products = productsQuery.Take(20).ToList();

            var viewModel = new HomeIndexViewModel
            {
                Categories = categories,
                Products = products
            };

            var notificationList = new List<string>();
            if (User.Identity.IsAuthenticated)
            {
                var userId = _context.Users.FirstOrDefault(u => u.UserName == User.Identity.Name)?.Id;
                if (userId != null)
                {
                    notificationList = _context.Notifications
                        .Where(n => n.UserId == userId && !n.IsRead)
                        .OrderByDescending(n => n.CreatedAt)
                        .Select(n => n.Message)
                        .ToList();
                }
            }
            if (TempData["Notification"] != null)
                notificationList.Add(TempData["Notification"].ToString());
            ViewBag.NotificationList = notificationList;
            ViewBag.Search = search;
            return View(viewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        [HttpPost]
        public async Task<IActionResult> SetLanguage(string culture, string returnUrl = null)
        {
            if (!string.IsNullOrEmpty(culture))
            {
                Response.Cookies.Append(".AspNetCore.Culture", $"c={culture}|uic={culture}");
                if (User.Identity.IsAuthenticated)
                {
                    var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
                    if (user != null)
                    {
                        user.Language = culture;
                        _context.Users.Update(user);
                        await _context.SaveChangesAsync();
                    }
                }
            }
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            return RedirectToAction("Index", "Home");
        }
    }
}
