using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DALTW.Models
{

    /// Lớp ngữ cảnh cơ sở dữ liệu (Database Context) chính của ứng dụng.
    /// Kế thừa từ IdentityDbContext để tích hợp sẵn các bảng quản lý người dùng, vai trò (Roles),
    /// và các tính năng xác thực từ thư viện ASP.NET Core Identity.
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {

        /// Phương thức khởi tạo (Constructor) nhận các tùy chọn cấu hình cơ sở dữ liệu
        /// từ file Program.cs (như chuỗi kết nối SQL Server).
        public ApplicationDbContext(DbContextOptions options) : base(options) { }

   
        // DANH SÁCH CÁC BẢNG (TABLES) TRONG CƠ SỞ DỮ LIỆU
        // Mỗi DbSet đại diện cho một bảng trong Database.
       

        /// Bảng quản lý thông tin chính của các Phòng khách sạn.
        public DbSet<Room> Rooms { get; set; }

        /// Bảng phân loại phòng (VD: Single, Double, Deluxe, VIP...). 
        public DbSet<RoomType> RoomTypes { get; set; }

        /// Bảng lưu trữ các đơn đặt phòng (Booking) của khách hàng. 
        public DbSet<Booking> Bookings { get; set; }

        /// Bảng chi tiết các phòng được đặt trong một đơn đặt (mối quan hệ 1-nhiều). 
        public DbSet<BookingDetail> BookingDetails { get; set; }

        /// Bảng quản lý trạng thái khóa phòng (nhằm mục đích bảo trì hoặc giữ chỗ trước). 
        public DbSet<RoomLock> RoomLocks { get; set; }

        /// Bảng lưu trữ danh sách hình ảnh phong phú (Gallery) cho từng phòng. 
        public DbSet<RoomImage> RoomImages { get; set; }

        /// Bảng danh mục các dịch vụ tiện ích đi kèm (VD: Massage, Ăn sáng, Giặt là...). 
        public DbSet<Service> Services { get; set; }

        /// Bảng liên kết giữa Đơn đặt phòng và các Dịch vụ đã chọn. 
        public DbSet<BookingService> BookingServices { get; set; }

        /// Bảng quản lý các Tầng (Floors).
        public DbSet<Floor> Floors { get; set; }

        /// Bảng quản lý Loại tiện nghi (AmenityTypes).
        public DbSet<AmenityType> AmenityTypes { get; set; }

        /// Bảng quản lý các Tiện nghi cụ thể (Amenities).
        public DbSet<Amenity> Amenities { get; set; }

        /// Bảng quản lý Phiếu lắp đặt tiện nghi (AmenityInstallationSlips).
        public DbSet<AmenityInstallationSlip> AmenityInstallationSlips { get; set; }

        /// Bảng quản lý Phiếu sử dụng dịch vụ (ServiceUsageSlips).
        public DbSet<ServiceUsageSlip> ServiceUsageSlips { get; set; }

        /// Bảng quản lý các Sản phẩm bán hàng (Products).
        public DbSet<Product> Products { get; set; }

        /// Bảng quản lý Phiếu bán hàng (ProductSaleSlips).
        public DbSet<ProductSaleSlip> ProductSaleSlips { get; set; }

        /// Bảng quản lý Phiếu đền bù (CompensationSlips).
        public DbSet<CompensationSlip> CompensationSlips { get; set; }

        /// Bảng quản lý Hóa đơn (Invoices).
        public DbSet<Invoice> Invoices { get; set; }
    }
}