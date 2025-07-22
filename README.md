# FigureShop

## 🏣️ Tổng quan

**FigureShop** là một website bán mô hình (figure) được xây dựng bằng **ASP.NET Core MVC**. Dự án này hỗ trợ các chức năng cơ bản của một hệ thống thương mại điện tử như: quản lý sản phẩm, giỏ hàng, đặt hàng, người dùng, phân loại sản phẩm, khuyến mãi, v.v.

---

## ✅ Những gì đã hoàn thành

### 🧩 Chức năng chính

* **Quản lý người dùng:**

  * Đăng ký, đăng nhập, đăng xuất
  * Phân quyền Admin / User
* **Quản lý sản phẩm:**

  * Thêm / Sửa / Xoá / Xem chi tiết
  * Upload nhiều ảnh (ImagePath, ImagePath2, ImagePath3)
  * Hỗ trợ hiển thị giá khuyến mãi (`DiscountPrice`)
* **Quản lý danh mục (Category):**

  * Thêm / sửa / xoá danh mục
  * Lọc sản phẩm theo danh mục
* **Giỏ hàng (ShoppingCart):**

  * Thêm sản phẩm vào giỏ
  * Cập nhật số lượng
  * Hiển thị đơn giá khuyến mãi nếu có
  * Tính tổng tiền chính xác theo giá sale
* **Đặt hàng & Checkout:**

  * Tạo đơn hàng giả lập
  * Xem tổng tiền và danh sách sản phẩm đã chọn
* **Trang chủ:**

  * Hiển thị danh mục nổi bật
  * Hiển thị sản phẩm mới, sản phẩm khuyến mãi (sale), sản phẩm hot
  * Tối ưu hiển thị giá thường và giá khuyến mãi
* **Khác:**

  * View sử dụng Razor kết hợp Bootstrap 5
  * AJAX cập nhật số lượng giỏ hàng
  * Migration đầy đủ bằng Entity Framework Core
  * Code chia rõ theo tầng Model - View - Controller

---

## 🗂️ Cấu trúc thư mục

```
Controllers/         // Xử lý logic (Product, ShoppingCart, Order, User...)
Models/              // Các entity EF Core (Product, Order, etc.)
ViewModels/          // Dùng để hiển thị hoặc binding nhiều model
Views/               // Razor View theo thư mục Controller
wwwroot/             // Tài nguyên tĩnh: ảnh, css, js
Migrations/          // File migration của Entity Framework
Program.cs           // Cấu hình khởi động ứng dụng
appsettings.json     // Cấu hình chuỗi kết nối và môi trường
```

---

## 🚀 Hướng dẫn chạy dự án

1. **Clone repo:**

   ```bash
   git clone https://github.com/ErenKid/FigureShop.git
   ```
2. **Mở project bằng Visual Studio**
3. **Restore NuGet packages**
4. **Chạy migration (nếu chưa có CSDL):**

   ```bash
   dotnet ef database update
   ```

   Hoặc trong Visual Studio:

   ```
   Tools > NuGet Package Manager > Package Manager Console
   > Update-Database
   ```
5. **F5 hoặc Ctrl + F5 để chạy ứng dụng**

---

## 🧐 Ghi chú kỹ thuật

* Sử dụng `decimal? DiscountPrice` để hỗ trợ sản phẩm có hoặc không có giá khuyến mãi
* Tính toán giá sale khi thêm vào giỏ hàng
* View sử dụng điều kiện `if (DiscountPrice != null)` để hiển thị phù hợp
* Giỏ hàng lưu đơn giá `Price` đúng tại thời điểm thêm (tránh bị thay đổi nếu sau này chỉnh giá)

---

## 🤝 Đóng góp

Mọ i đóng góp, câu hỏi hoặc báo lỗi xin gửi **issue** hoặc **pull request** tại repository:

🔗 [https://github.com/ErenKid/FigureShop](https://github.com/ErenKid/FigureShop)
