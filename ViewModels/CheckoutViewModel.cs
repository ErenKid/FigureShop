namespace FigureShop.ViewModels
{
    public class CheckoutViewModel
    {
        public string FullName { get; set; } = null!;
        public string Address { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public string? Note { get; set; }
        public decimal TotalAmount { get; set; }
        public List<FigureShop.Models.ShoppingCartItem> CartItems { get; set; } = new();
        public string PaymentMethod { get; set; } = "COD";
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string? VoucherCode { get; set; } // Mã giảm giá từ form
    }
}