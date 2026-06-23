using DALTW.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DALTW.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Staff")]
    public class InvoiceController : Controller
    {
        private readonly ApplicationDbContext _context;

        public InvoiceController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ================= INDEX (GET) =================
        public async Task<IActionResult> Index(string? keyword, string? paymentMethod, DateTime? startDate, DateTime? endDate)
        {
            var query = _context.Invoices
                .Include(i => i.Booking)
                    .ThenInclude(b => b.User)
                .AsQueryable();

            // 1. TÌM KIẾM THEO KEYWORD
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(i =>
                    i.Id.ToString() == keyword ||
                    (i.Booking != null && i.Booking.CustomerName.Contains(keyword)) ||
                    (i.Booking != null && i.Booking.CustomerEmail.Contains(keyword)) ||
                    (i.Booking != null && i.Booking.CustomerPhone.Contains(keyword))
                );
            }

            // 2. LỌC THEO PHƯƠNG THỨC THANH TOÁN
            if (!string.IsNullOrWhiteSpace(paymentMethod))
            {
                query = query.Where(i => i.PaymentMethod == paymentMethod);
            }

            // 3. LỌC THEO KHOẢNG THỜI GIAN
            if (startDate.HasValue)
            {
                query = query.Where(i => i.PaymentDate >= startDate.Value.Date);
            }
            if (endDate.HasValue)
            {
                // Thêm 1 ngày để bao gồm tất cả giao dịch trong ngày kết thúc
                var endLimit = endDate.Value.Date.AddDays(1);
                query = query.Where(i => i.PaymentDate < endLimit);
            }

            var invoices = await query
                .OrderByDescending(i => i.PaymentDate)
                .ToListAsync();

            // Tính thống kê nhanh
            ViewBag.TotalCount = invoices.Count;
            ViewBag.TotalRevenue = invoices.Sum(i => i.TotalAmount);

            // Giữ lại trạng thái bộ lọc trên View
            ViewBag.Keyword = keyword;
            ViewBag.PaymentMethod = paymentMethod;
            ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");

            return View(invoices);
        }

        // ================= DETAILS (GET) =================
        public async Task<IActionResult> Details(int id)
        {
            var invoice = await _context.Invoices
                .Include(i => i.Booking)
                    .ThenInclude(b => b.BookingDetails)
                        .ThenInclude(d => d.Room)
                            .ThenInclude(r => r.RoomType)
                .Include(i => i.Booking)
                    .ThenInclude(b => b.User)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (invoice == null) return NotFound();

            // Nạp các chi tiết dịch vụ, sản phẩm và đền bù
            ViewBag.ServiceSlips = await _context.ServiceUsageSlips
                .Include(s => s.Service)
                .Where(s => s.BookingId == invoice.BookingId)
                .ToListAsync();

            ViewBag.ProductSlips = await _context.ProductSaleSlips
                .Include(p => p.Product)
                .Where(p => p.BookingId == invoice.BookingId)
                .ToListAsync();

            ViewBag.CompensationSlips = await _context.CompensationSlips
                .Include(c => c.Amenity)
                    .ThenInclude(a => a.AmenityType)
                .Where(c => c.BookingId == invoice.BookingId)
                .ToListAsync();

            return View(invoice);
        }

        // ================= DELETE (POST) =================
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")] // Chỉ Admin mới được xóa hóa đơn
        public async Task<IActionResult> Delete(int id)
        {
            var invoice = await _context.Invoices.FindAsync(id);
            if (invoice == null) return NotFound();

            _context.Invoices.Remove(invoice);
            await _context.SaveChangesAsync();

            TempData["success"] = "🗑️ Đã xóa hóa đơn thành công khỏi hệ thống!";
            return RedirectToAction(nameof(Index));
        }
    }
}
