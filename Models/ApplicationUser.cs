using Microsoft.AspNetCore.Identity;

namespace FigureShop.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? FullName { get; set; }
        public string? Address { get; set; }
        // Thêm các thuộc tính khác nếu cần
    }
}
