using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FigureShop.Models;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using FigureShop.ViewModels;

namespace FigureShop.Controllers
{
 
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
        public async Task<IActionResult> Index(int? pageNumber, string searchString, string sortOrder)
        {
            int pageSize = 10;
            var users = from u in _context.Users select u;

            if (!string.IsNullOrEmpty(searchString))
            {
                users = users.Where(s => s.UserName.Contains(searchString) || s.Email.Contains(searchString));
            }

            switch (sortOrder)
            {
                case "az":
                    users = users.OrderBy(u => u.UserName);
                    break;
                case "za":
                    users = users.OrderByDescending(u => u.UserName);
                    break;
                default:
                    users = users.OrderBy(u => u.UserName);
                    break;
            }

            int page = pageNumber ?? 1;
            var paginatedUsers = await users
                                       .Skip((page - 1) * pageSize)
                                       .Take(pageSize)
                                       .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.SearchString = searchString;
            ViewBag.TotalPages = (int)Math.Ceiling((double)users.Count() / pageSize);
            ViewBag.SortOrder = sortOrder;

            return View(paginatedUsers);
        }

        // GET: User/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

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
                    // Tự động xác nhận email nếu tạo từ admin
                    var emailToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    await _userManager.ConfirmEmailAsync(user, emailToken);
                    return RedirectToAction(nameof(Index));
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return View(model);
        }

        // GET: User/ChangePassword/5
        [HttpGet]
        public async Task<IActionResult> ChangePassword(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();
            ViewBag.UserId = id;
            return View(new ChangePasswordViewModel());
        }

        // POST: User/ChangePassword/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(string userId, ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.UserId = userId;
                return View(model);
            }
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                ViewBag.Error = "Người dùng không tồn tại.";
                ViewBag.UserId = userId;
                return View(model);
            }
            var result = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
            if (result.Succeeded)
            {
                ViewBag.Message = "Đổi mật khẩu thành công!";
                ViewBag.UserId = userId;
                return View(new ChangePasswordViewModel());
            }
            else
            {
                ViewBag.Error = string.Join("<br>", result.Errors.Select(e => e.Description));
                ViewBag.UserId = userId;
                return View(model);
            }
        }
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            return View(user);
        }

        // POST: User/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, ApplicationUser model)
        {
            if (id != model.Id) return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            // Cập nhật thông tin
            user.FullName = model.FullName;
            user.Address = model.Address;
            user.PhoneNumber = model.PhoneNumber;

            // Xử lý ảnh nếu có upload
            var file = Request.Form.Files["Avatar"];
            if (file != null && file.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/avatars");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                user.AvatarPath = "/uploads/avatars/" + fileName;
            }

            await _userManager.UpdateAsync(user);
            // Lưu thông báo vào DB
            var notification = new Notification {
                UserId = user.Id,
                Message = "Cập nhật thông tin thành công!",
                CreatedAt = DateTime.Now,
                IsRead = false,
                OrderId = null
            };
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
            // Quay lại trang chi tiết
            return RedirectToAction("Details", new { id = user.Id });
        }

        // GET: User/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            return View(user);
        }

        // POST: User/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                await _userManager.DeleteAsync(user);
            }
            return RedirectToAction(nameof(Index));
        }

        private bool UserExists(string id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }
}
