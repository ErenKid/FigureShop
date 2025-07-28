using System;
using System.ComponentModel.DataAnnotations;
using FigureShop.Models;

namespace FigureShop.Models
{
    public class ProductReview
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; }
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        [Range(1,5)]
        public int Rating { get; set; }
        [Required]
        public string Comment { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
