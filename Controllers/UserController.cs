using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FigureShop.Models;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using FigureShop.ViewModels;

namespace FigureShop.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UserController : Controller
    {
        private readonly FigureShopContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public UserController(FigureShopContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: User
        public async Task<IActionResult> Index(int? pageNumber, string searchString)
        {
            int pageSize = 10;
            var users = from u in _context.Users
                        select u;

            if (!string.IsNullOrEmpty(searchString))
            {
                users = users.Where(s => s.UserName.Contains(searchString) || s.Email.Contains(searchString));
            }

            int page = pageNumber ?? 1;
            var paginatedUsers = await users
                                       .Skip((page - 1) * pageSize)
                                       .Take(pageSize)
                                       .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.SearchString = searchString;
            ViewBag.TotalPages = (int)Math.Ceiling((double)users.Count() / pageSize);

            return View(paginatedUsers);
        }

        // GET: User/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(m => m.Id == id.ToString());
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // GET: User/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: User/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.UserName,
                    Email = model.Email,
                    FullName = model.FullName,
                    Address = model.Address,
                    PhoneNumber = model.PhoneNumber
                };
                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    return RedirectToAction(nameof(Index));
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return View(model);
        }

        // GET: User/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        // POST: User/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Username,Email,PasswordHash,FullName,Address,Phone,Role,CreatedAt,UpdatedAt")] User user)
        {
            if (id != user.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                // Giữ mật khẩu cũ nếu không nhập mới
                if (string.IsNullOrEmpty(user.PasswordHash))
                {
                    var oldUser = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id.ToString());
                    user.PasswordHash = oldUser?.PasswordHash;
                }
                try
                {
                    _context.Update(user);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(user.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(user);
        }

        // GET: User/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(m => m.Id == id.ToString());
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: User/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool UserExists(string id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }
}
