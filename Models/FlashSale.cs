using System;
using System.ComponentModel.DataAnnotations;

namespace FigureShop.Models
{
    public class FlashSale
    {
        public int Id { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Range(0.01, 1)]
        public decimal Discount { get; set; } // Ví dụ: 0.2 = 20%

        [Required]
        public DateTime StartTime { get; set; }

        [Required]
        public DateTime EndTime { get; set; }

        public Product? Product { get; set; }
    }
}
