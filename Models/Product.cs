using System;

namespace FigureShop.Models
{
    public class Product
    {
        public int Id { get; set; }               // Khóa chính của sản phẩm
        public string Name { get; set; } = null!;           // Tên sản phẩm
        public string Description { get; set; } = null!;    // Mô tả sản phẩm
        public decimal Price { get; set; }         // Giá sản phẩm
        public int CategoryId { get; set; }           // Khóa ngoại đến Category
        public Category? Category { get; set; }       // Navigation property
        public string ImagePath { get; set; } = null!;      // Đường dẫn ảnh sản phẩm
        public string? ImagePath2 { get; set; }             // Đường dẫn ảnh phụ 2
        public string? ImagePath3 { get; set; }             // Đường dẫn ảnh phụ 3
        public int Stock { get; set; }        
        public decimal? DiscountPrice { get; set; }     // Số lượng tồn kho
    }
}
