using Microsoft.AspNetCore.Identity;

namespace FigureShop.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? FullName { get; set; }
        public string? Address { get; set; }
    // Thêm các thuộc tính khác nếu cần
    public string? AvatarPath { get; set; }// Đường dẫn đến ảnh đại diện của người dùng
    public string? Language { get; set; } // Ngôn ngữ của người dùng
    }
}
