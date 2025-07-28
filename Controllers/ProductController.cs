using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using OfficeOpenXml;
using FigureShop.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.IO;
using System;

namespace FigureShop.Controllers
{
    [Authorize]
    public class ProductController : Controller
    {
        private readonly FigureShopContext _context;

        public ProductController(FigureShopContext context)
        {
            _context = context;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Search(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
                return Json(new List<object>());

            var products = await _context.Products
                .Where(p => p.Name.ToLower().Contains(term.ToLower()))
                .OrderByDescending(p => p.Id)
                .Take(10)
                .Select(p => new {
                    id = p.Id,
                    name = p.Name,
                    price = p.Price,
                    imagePath = p.ImagePath
                })
                .ToListAsync();

            return Json(products);
        }

        // GET: Product
        public async Task<IActionResult> Index(string searchString, int? categoryId, int? pageNumber)
        {
            int pageSize = 8;
            var products = _context.Products.Include(p => p.Category).AsQueryable();

            if (categoryId.HasValue)
                products = products.Where(p => p.CategoryId == categoryId);

            if (!string.IsNullOrEmpty(searchString))
                products = products.Where(s => s.Name.Contains(searchString) || s.Category.Name.Contains(searchString));

            // Lọc theo sortOrder
            string sortOrder = Request.Query["sortOrder"];
            // Lọc nâng cao
            decimal minPrice = 0;
            decimal maxPrice = 0;
            if (decimal.TryParse(Request.Query["minPrice"], out var min)) minPrice = min;
            if (decimal.TryParse(Request.Query["maxPrice"], out var max)) maxPrice = max;
            string status = Request.Query["status"];
            string sale = Request.Query["sale"];

            if (minPrice > 0)
                products = products.Where(p => (p.DiscountPrice ?? p.Price) >= minPrice);
            if (maxPrice > 0)
                products = products.Where(p => (p.DiscountPrice ?? p.Price) <= maxPrice);

            if (!string.IsNullOrEmpty(status))
            {
                switch (status)
                {
                    case "available":
                        products = products.Where(p => p.Stock > 0 && !p.IsPreOrder);
                        break;
                    case "outofstock":
                        products = products.Where(p => p.Stock == 0);
                        break;
                    case "preorder":
                        products = products.Where(p => p.IsPreOrder);
                        break;
                }
            }

            if (!string.IsNullOrEmpty(sale))
            {
                switch (sale)
                {
                    case "sale":
                        products = products.Where(p => p.DiscountPrice != null && p.DiscountPrice < p.Price);
                        break;
                    case "flashsale":
                        products = products.Where(p => p.IsFlashSale);
                        break;
                }
            }

            switch (sortOrder)
            {
                case "az":
                    products = products.OrderBy(p => p.Name);
                    break;
                case "za":
                    products = products.OrderByDescending(p => p.Name);
                    break;
                case "priceasc":
                    products = products.OrderBy(p => p.DiscountPrice ?? p.Price);
                    break;
                case "pricedesc":
                    products = products.OrderByDescending(p => p.DiscountPrice ?? p.Price);
                    break;
                default:
                    products = products.OrderBy(p => p.Name);
                    break;
            }

            var categories = await _context.Categories.ToListAsync();
            ViewBag.Categories = categories;
            ViewBag.SelectedCategoryId = categoryId;
            ViewBag.SortOrder = sortOrder;

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

            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductReviews).ThenInclude(r => r.User)
                .Include(p => p.ProductComments).ThenInclude(c => c.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (product == null)
                return NotFound();

            var upSaleProducts = await _context.Products
                .Where(p => p.DiscountPrice != null && p.DiscountPrice > 0 && p.DiscountPrice < p.Price)
                .OrderByDescending(p => p.Id)
                .Take(6)
                .ToListAsync();
            ViewBag.UpSaleProducts = upSaleProducts;

            return View(product);
        }

        // GET: Product/BulkUpload
        [Authorize(Roles = "Admin")]
        public IActionResult BulkUpload()
        {
            return View();
        }

        // POST: Product/BulkUpload
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> BulkUpload(IFormFile excelFile)
        {
            if (excelFile == null || excelFile.Length == 0)
            {
                TempData["Error"] = "Vui lòng chọn file Excel.";
                return View();
            }

            var products = new List<Product>();
            var validCategoryIds = _context.Categories.Select(c => c.Id).ToHashSet();
            int skippedRows = 0;
            using (var stream = new MemoryStream())
            {
                await excelFile.CopyToAsync(stream);
                stream.Position = 0;
                ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
                using (var package = new ExcelPackage(stream))
                {
                    var worksheet = package.Workbook.Worksheets.First();
                    int rowCount = worksheet.Dimension.Rows;
                    for (int row = 2; row <= rowCount; row++)
                    {
                        int catId = int.TryParse(worksheet.Cells[row, 4].Text, out var tmpCatId) ? tmpCatId : 0;
                        if (!validCategoryIds.Contains(catId))
                        {
                            skippedRows++;
                            continue; // Bỏ qua sản phẩm có CategoryId không hợp lệ
                        }
                        var product = new Product
                        {
                            Name = worksheet.Cells[row, 1].Text,
                            Description = worksheet.Cells[row, 2].Text,
                            Price = decimal.TryParse(worksheet.Cells[row, 3].Text, out var price) ? price : 0,
                            CategoryId = catId,
                            ImagePath = worksheet.Cells[row, 5].Text,
                            Stock = int.TryParse(worksheet.Cells[row, 6].Text, out var stock) ? stock : 0,
                            DiscountPrice = decimal.TryParse(worksheet.Cells[row, 7].Text, out var discount) ? discount : null,
                            IsPreOrder = worksheet.Cells[row, 8].Text == "1" || worksheet.Cells[row, 8].Text.ToLower() == "true"
                        };
                        products.Add(product);
                    }
                }
            }
            _context.Products.AddRange(products);
            await _context.SaveChangesAsync();
            TempData["Success"] = $"Đã thêm {products.Count} sản phẩm thành công. {skippedRows} dòng bị bỏ qua do CategoryId không hợp lệ.";
            return RedirectToAction("Index");
        }

        // GET: Product/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var product = await _context.Products
                .Include(p => p.FlashSale)
                .FirstOrDefaultAsync(p => p.Id == id);

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

            var discountStr = Request.Form["FlashSale.Discount"];
            var startStr = Request.Form["FlashSale.StartTime"];
            var endStr = Request.Form["FlashSale.EndTime"];
            var cancelFlash = Request.Form["FlashSale.Cancel"];

            if (ModelState.IsValid)
            {
                try
                {
                    var category = await _context.Categories.FindAsync(product.CategoryId);
                    product.IsPreOrder = category?.Name.Trim().ToLower() == "pre-order";

                    _context.Update(product);

                    var existingFlash = await _context.FlashSales.FirstOrDefaultAsync(f => f.ProductId == product.Id);

                    if (cancelFlash == "true")
                    {
                        if (existingFlash != null)
                            _context.FlashSales.Remove(existingFlash);
                    }
                    else if (
                        decimal.TryParse(discountStr, out decimal discount) && discount > 0 &&
                        DateTime.TryParse(startStr, out DateTime startTime) &&
                        DateTime.TryParse(endStr, out DateTime endTime))
                    {
                        if (existingFlash != null)
                        {
                            existingFlash.Discount = discount;
                            existingFlash.StartTime = startTime;
                            existingFlash.EndTime = endTime;
                            _context.Update(existingFlash);
                        }
                        else
                        {
                            var newFlash = new FlashSale
                            {
                                ProductId = product.Id,
                                Discount = discount,
                                StartTime = startTime,
                                EndTime = endTime
                            };
                            _context.FlashSales.Add(newFlash);
                        }
                    }

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

            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.Id == id);

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

        // GET: Product/Create
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Categories = _context.Categories.ToList();
            return View();
        }

        // POST: Product/Create
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Description,Price,DiscountPrice,CategoryId,ImagePath,ImagePath2,ImagePath3,Stock")] Product product)
        {
            if (ModelState.IsValid)
            {
                _context.Products.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.Categories = _context.Categories.ToList();
            return View(product);
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }
    }
}
