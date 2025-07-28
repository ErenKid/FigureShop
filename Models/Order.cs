using System;
using System.Collections.Generic;

namespace FigureShop.Models
{
    public class Order
    {
        public int Id { get; set; }
        public string UserId { get; set; } = null!;
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = null!; // Trạng thái: Pending, Preparing, Shipped, Completed, Cancelled
        public string PaymentMethod { get; set; } = null!;
        public ApplicationUser User { get; set; } = null!;
        public List<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public string Address { get; set; } = null!;
        public string? VoucherCode { get; set; } // Mã giảm giá áp dụng
        public decimal VoucherDiscount { get; set; } // Số tiền giảm từ voucher
    }
}
