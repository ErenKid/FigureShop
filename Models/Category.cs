using System;
using System.ComponentModel.DataAnnotations;

namespace FigureShop.Models
{
    public class Category
    {
        public int Id { get; set; }              // Khóa chính của Category
        public string Name { get; set; }          // Tên danh mục
        public string Description { get; set; }   // Mô tả về danh mục
        public DateTime CreatedAt { get; set; }   // Thời gian tạo danh mục
        public DateTime UpdatedAt { get; set; }   // Thời gian cập nhật danh mục
    }
} 