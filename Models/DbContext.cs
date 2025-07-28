using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using FigureShop.Models;


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
        public DbSet<FlashSale>? FlashSales { get; set; }
        public DbSet<ProductReview> ProductReviews { get; set; }
        public DbSet<ProductComment> ProductComments { get; set; }
        public DbSet<Notification> Notifications { get; set; }
    
        // Nếu cần giữ bảng Users riêng, có thể thêm DbSet<User>? Users { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);

    // Mối quan hệ 1-1 giữa Product và FlashSale
    modelBuilder.Entity<FlashSale>()
        .HasOne<Product>()
        .WithOne(p => p.FlashSale)
        .HasForeignKey<FlashSale>(f => f.ProductId)
        .OnDelete(DeleteBehavior.Cascade); // Xoá sản phẩm sẽ xoá luôn flashsale nếu cần
}
    }
}
