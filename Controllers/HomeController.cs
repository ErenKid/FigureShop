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

    var products = _context.Products
        .Include(p => p.Category) // 👈 BẮT BUỘC
        .OrderByDescending(p => p.Id)
        .Take(20)
        .ToList();

    var viewModel = new HomeIndexViewModel
    {
        Categories = categories,
        Products = products
    };

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
    }
}
