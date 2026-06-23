using DALTW.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DALTW.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Staff")]
    public class BookingManageController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BookingManageController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ================= INDEX =================
        public async Task<IActionResult> Index(string? status, string? keyword, int? month, int? year)
        {
            var query = _context.Bookings
                .Include(b => b.User)
                .Include(b => b.BookingDetails)
                    .ThenInclude(d => d.Room)
                        .ThenInclude(r => r.RoomType)
                .AsQueryable();

            // FILTER STATUS
            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(b => b.Status == status);

            // FILTER KEYWORD
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(b =>
                    (b.CustomerName != null && b.CustomerName.Contains(keyword)) ||
                    (b.User != null && b.User.FullName.Contains(keyword)) ||
                    (b.User != null && b.User.Email.Contains(keyword))
                );
            }

            // FILTER MONTH
            if (month.HasValue)
                query = query.Where(b => b.CheckIn.Month == month.Value);

            // FILTER YEAR
            if (year.HasValue)
                query = query.Where(b => b.CheckIn.Year == year.Value);

            var bookings = await query
                .OrderByDescending(b => b.Id)
                .ToListAsync();

            ViewBag.Status = status;
            ViewBag.Keyword = keyword;
            ViewBag.Month = month;
            ViewBag.Year = year;

            ViewBag.TotalBookings = bookings.Count;
            ViewBag.TotalRevenue = bookings
                .Where(b => b.Status == BookingStatus.Confirmed.ToString() || b.Status == BookingStatus.CheckedIn.ToString() || b.Status == BookingStatus.Completed.ToString())
                .Sum(b => b.TotalPrice);

            return View(bookings);
        }

        // ================= DETAILS =================
        public async Task<IActionResult> Details(int id)
        {
            var booking = await _context.Bookings
                .Include(b => b.User)
                .Include(b => b.BookingDetails)
                    .ThenInclude(d => d.Room)
                        .ThenInclude(r => r.RoomType)
                .Include(b => b.BookingServices)
                    .ThenInclude(bs => bs.Service)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (booking == null)
                return NotFound();

            return View(booking);
        }

        // ================= CONFIRM =================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Confirm(int id)
        {
            var booking = await _context.Bookings
                .Include(b => b.BookingDetails)
                    .ThenInclude(d => d.Room)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (booking == null)
                return NotFound();

            if (booking.Status != BookingStatus.Pending.ToString())
                return BadRequest("Booking không hợp lệ để xác nhận.");

            booking.Status = BookingStatus.Confirmed.ToString();

            foreach (var detail in booking.BookingDetails)
            {
                if (detail.Room != null)
                {
                    detail.Room.IsAvailable = false;
                    detail.Room.Status = "Đã đặt";
                }
            }

            await _context.SaveChangesAsync();

            TempData["success"] = "✅ Xác nhận booking thành công!";
            return RedirectToAction(nameof(Index));
        }

        // ================= CANCEL =================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var booking = await _context.Bookings
                .Include(b => b.BookingDetails)
                    .ThenInclude(d => d.Room)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (booking == null)
                return NotFound();

            booking.Status = BookingStatus.Cancelled.ToString();

            foreach (var detail in booking.BookingDetails)
            {
                if (detail.Room != null)
                {
                    detail.Room.IsAvailable = true;
                    detail.Room.Status = "Trống";
                }
            }

            await _context.SaveChangesAsync();

            TempData["success"] = "⚠️ Đã hủy booking!";
            return RedirectToAction(nameof(Index));
        }

        // ================= DELETE =================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var booking = await _context.Bookings
                .Include(b => b.BookingDetails)
                    .ThenInclude(d => d.Room)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (booking == null)
                return NotFound();

            // nếu booking đã confirm hoặc checked-in → mở lại phòng
            if (booking.Status == BookingStatus.Confirmed.ToString() || booking.Status == BookingStatus.CheckedIn.ToString())
            {
                foreach (var detail in booking.BookingDetails)
                {
                    if (detail.Room != null)
                    {
                        detail.Room.IsAvailable = true;
                        detail.Room.Status = "Trống";
                    }
                }
            }

            _context.Bookings.Remove(booking);
            await _context.SaveChangesAsync();

            TempData["success"] = "🗑️ Xóa booking thành công!";
            return RedirectToAction(nameof(Index));
        }
    }

    // ================= ENUM STATUS (RẤT QUAN TRỌNG) =================
    public enum BookingStatus
    {
        Pending,
        Confirmed,
        CheckedIn,
        Completed,
        Cancelled
    }
}