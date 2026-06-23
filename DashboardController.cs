using DALTW.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DALTW.Areas.Admin.Controllers
{
    /// Điều khiển (Controller) hiển thị Bảng điều khiển (Dashboard) trung tâm cho Quản trị viên và Nhân viên.
    /// Tổng hợp và trình bày các con số thống kê quan trọng về hoạt động của khách sạn.
    [Area("Admin")]
    [Authorize(Roles = "Admin,Staff")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        /// Khởi tạo Controller với Database Context.
        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// Trang chủ khu vực Admin: Thực hiện tính toán và hiển thị các chỉ số hiệu suất (KPIs).
        public async Task<IActionResult> Index()
        {
            // 1. Thống kê tổng quan số lượng các đối tượng chính trong hệ thống
            ViewBag.TotalRooms = await _context.Rooms.CountAsync();
            ViewBag.TotalUsers = await _context.Users.CountAsync();
            ViewBag.TotalBookings = await _context.Bookings.CountAsync();
            
            // 2. Thống kê chi tiết theo trạng thái đơn đặt phòng
            ViewBag.PendingBookings = await _context.Bookings.CountAsync(b => b.Status == "Pending");
            ViewBag.ConfirmedBookings = await _context.Bookings.CountAsync(b => b.Status == "Confirmed" || b.Status == "CheckedIn" || b.Status == "Completed");
            
            // 3. Tính toán tổng doanh thu tích lũy từ các Hóa đơn (Invoices)
            ViewBag.TotalRevenue = await _context.Invoices
                .SumAsync(i => (decimal?)i.TotalAmount) ?? 0;
            
            // 4. Số lượng phòng đang ở trạng thái 'Mở' (Trống/Sẵn sàng phục vụ)
            ViewBag.AvailableRooms = await _context.Rooms.CountAsync(r => r.IsAvailable);

            // 5. Thống kê doanh thu 12 tháng của năm hiện tại từ Hóa đơn (Invoices)
            var currentYear = DateTime.Today.Year;
            var monthlyData = await _context.Invoices
                .Where(i => i.PaymentDate.Year == currentYear)
                .GroupBy(i => i.PaymentDate.Month)
                .Select(g => new { Month = g.Key, Revenue = g.Sum(i => i.TotalAmount) })
                .ToListAsync();

            var monthlyRevenue = new decimal[12];
            foreach (var data in monthlyData)
            {
                if (data.Month >= 1 && data.Month <= 12)
                {
                    monthlyRevenue[data.Month - 1] = data.Revenue;
                }
            }
            ViewBag.MonthlyRevenue = monthlyRevenue;

            // 6. Thống kê Top 5 phòng được đặt nhiều nhất
            var topRoomsData = await _context.BookingDetails
                .Include(bd => bd.Booking)
                .Include(bd => bd.Room)
                .Where(bd => bd.Booking != null && (bd.Booking.Status == "Confirmed" || bd.Booking.Status == "CheckedIn" || bd.Booking.Status == "Completed"))
                .GroupBy(bd => bd.Room != null ? bd.Room.Name : "Chưa xác định")
                .Select(g => new { RoomName = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(5)
                .ToListAsync();

            ViewBag.TopRoomNames = topRoomsData.Select(x => x.RoomName).ToList();
            ViewBag.TopRoomCounts = topRoomsData.Select(x => x.Count).ToList();

            // 7. Truy vấn danh sách 5 đơn đặt phòng gần đây nhất
            var recentBookings = await _context.Bookings
                .Include(b => b.User)
                .Include(b => b.BookingDetails).ThenInclude(d => d.Room)
                .OrderByDescending(b => b.Id)
                .Take(5)
                .ToListAsync();

            return View(recentBookings);
        }
    }
}