namespace FigureShop.Models
{
    public class ShoppingCartItem
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public Product Product { get; set; } = null!;
        public int ShoppingCartId { get; set; } // Khóa ngoại đến ShoppingCart
        public ShoppingCart ShoppingCart { get; set; } = null!;
    }
} 