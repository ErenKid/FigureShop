using System;
namespace FigureShop.Models
{
    public class ProductComment
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string UserId { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }

        public Product Product { get; set; }
        public ApplicationUser User { get; set; }
    }
}
