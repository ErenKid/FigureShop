using System.Collections.Generic;

namespace FigureShop.Models
{
    public class ShoppingCart
    {
        public int Id { get; set; }
        public string UserId { get; set; } = null!;
        public List<ShoppingCartItem> Items { get; set; } = new List<ShoppingCartItem>();
        public decimal TotalAmount { get; set; }
    }
} 