using System;
using Microsoft.AspNetCore.Identity;

namespace FigureShop.Models
{
    public class User : IdentityUser
    {
        public string? FullName { get; set; }       // Tên đầy đủ của người dùng
        public string? Address { get; set; }        // Địa chỉ của người dùng
        public string? Role { get; set; }           // Vai trò của người dùng (Admin, User, v.v.)
        public DateTime CreatedAt { get; set; }    // Thời gian tạo tài khoản
        public DateTime UpdatedAt { get; set; }    // Thời gian cập nhật tài khoản
    }
}
