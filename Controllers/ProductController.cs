using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using FigureShop.Models;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Authorization;

namespace FigureShop.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ProductController : Controller
    {
        private readonly FigureShopContext _context;

        public ProductController(FigureShopContext context)
        {
            _context = context;
        }

        // GET: Product
        public async Task<IActionResult> Index(string searchString, int? categoryId, int? pageNumber)
        {
            int pageSize = 12;
            var products = _context.Products.Include(p => p.Category).AsQueryable();

            // Filter by category
            if (categoryId.HasValue)
            {
                products = products.Where(p => p.CategoryId == categoryId);
            }

            // Search functionality
            if (!string.IsNullOrEmpty(searchString))
            {
                products = products.Where(s => s.Name.Contains(searchString) || s.Category.Name.Contains(searchString));
            }

            // Get categories for sidebar
            var categories = await _context.Categories.ToListAsync();
            ViewBag.Categories = categories;
            ViewBag.SelectedCategoryId = categoryId;

            // Pagination
            int totalCount = await products.CountAsync();
            int currentPage = pageNumber ?? 1;
            var paginatedProducts = await products
                .Skip((currentPage - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentFilter = searchString;
            ViewBag.PageNumber = currentPage;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalCount = totalCount;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            return View(paginatedProducts);
        }

        // GET: Product/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();
            var product = await _context.Products.Include(p => p.Category).FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
                return NotFound();
            return View(product);
        }

        // GET: Product/Create
        public IActionResult Create()
        {
            ViewBag.CategoryId = new SelectList(_context.Categories, "Id", "Name");
            return View();
        }

        // POST: Product/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Description,Price,DiscountPrice,CategoryId,ImagePath,ImagePath2,ImagePath3,Stock")] Product product)
        {
            if (ModelState.IsValid)
            {
                _context.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.CategoryId = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        // GET: Product/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return NotFound();
            ViewBag.CategoryId = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        // POST: Product/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,Price,DiscountPrice,CategoryId,ImagePath,ImagePath2,ImagePath3,Stock")] Product product)
        {
            if (id != product.Id)
                return NotFound();
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(product);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewBag.CategoryId = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        // GET: Product/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();
            var product = await _context.Products.Include(p => p.Category).FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
                return NotFound();
            return View(product);
        }

        // POST: Product/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }
    }
} 