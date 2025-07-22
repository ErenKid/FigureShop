using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FigureShop.Models
{
    public class FigureShopContext : IdentityDbContext<ApplicationUser>
    {
        public FigureShopContext(DbContextOptions<FigureShopContext> options) : base(options) { }

        public DbSet<Product>? Products { get; set; }
        public DbSet<Order>? Orders { get; set; }
        public DbSet<Category>? Categories { get; set; }
        public DbSet<ShoppingCart> ShoppingCarts { get; set; }
        public DbSet<ShoppingCartItem> ShoppingCartItems { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        // Nếu cần giữ bảng Users riêng, có thể thêm DbSet<User>? Users { get; set; }
    }
}
