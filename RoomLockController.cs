using DALTW.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DALTW.Areas.Admin.Controllers
{
    /// Controller quản lý các lệnh khóa phòng theo khoảng thời gian (Room Locking).
    [Area("Admin")]
    [Authorize(Roles = "Admin,Staff")]
    public class RoomLockController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RoomLockController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// Hiển thị danh sách tất cả các lệnh khóa phòng đang có.
        public async Task<IActionResult> Index()
        {
            var locks = await _context.RoomLocks
                .Include(r => r.Room)
                .OrderByDescending(r => r.FromDate)
                .ToListAsync();
            return View(locks);
        }

        public IActionResult Create(int roomId)
        {
            ViewBag.RoomId = roomId;
            return View();
        }
        /// Xử lý tạo lệnh khóa cho một phòng cụ thể.
        [HttpPost]
        public async Task<IActionResult> Create(RoomLock lockRoom)
        {
            _context.RoomLocks.Add(lockRoom);
            await _context.SaveChangesAsync();

            TempData["success"] = "✅ Đã lên lịch khóa phòng thành công!";
            return RedirectToAction(nameof(Index));
        }
        /// Khóa toàn bộ phòng trong hệ thống theo một khoảng thời gian cụ thể.
        /// Đồng thời cập nhật trạng thái IsAvailable = false để Dashboard và tìm kiếm ghi nhận là phòng đã 'Đóng'.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> LockAll(DateTime fromDate, DateTime toDate, string? reason)
        {
            // Kiểm tra tính hợp lệ của ngày tháng
            if (toDate <= fromDate)
            {
                TempData["error"] = "❌ Ngày kết thúc phải lớn hơn ngày bắt đầu!";
                return Redirect(Request.Headers["Referer"].ToString() ?? "/Admin/Room");
            }

            // Lấy danh sách toàn bộ phòng hiện có
            var rooms = await _context.Rooms.ToListAsync();

            if (!rooms.Any())
            {
                TempData["error"] = "❌ Không tìm thấy phòng nào để xử lý!";
                return Redirect(Request.Headers["Referer"].ToString() ?? "/Admin/Room");
            }

            // Xóa các lịch khóa (RoomLock) cũ có khoảng thời gian chồng lấn để tránh xung đột dữ liệu
            var existingLocks = await _context.RoomLocks
                .Where(l => l.FromDate < toDate && l.ToDate > fromDate)
                .ToListAsync();
            _context.RoomLocks.RemoveRange(existingLocks);

            // Duyệt qua từng phòng để tạo lịch khóa mới và cập nhật trạng thái IsAvailable
            foreach (var room in rooms)
            {
                // 1. Tạo bản ghi lịch khóa (Schedule Lock)
                _context.RoomLocks.Add(new RoomLock
                {
                    RoomId = room.Id,
                    FromDate = fromDate,
                    ToDate = toDate,
                    Reason = reason ?? "Đóng toàn hệ thống"
                });

                // 2. 🔥 QUAN TRỌNG: Cập nhật trạng thái phòng sang "Đóng" ngay lập tức
                // Giúp hệ thống Dashboard và chức năng tìm kiếm của khách hàng nhận diện ngay.
                room.IsAvailable = false; 
            }

            await _context.SaveChangesAsync();

            TempData["success"] = $"🏨 Đã chuyển trạng thái 'Đóng' cho toàn bộ {rooms.Count} phòng và cập nhật Dashboard!";

            // Quay lại trang trước đó nếu có
            var referer = Request.Headers["Referer"].ToString();
            if (!string.IsNullOrEmpty(referer)) return Redirect(referer);

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var lockItem = await _context.RoomLocks.FindAsync(id);
            if (lockItem != null)
            {
                _context.RoomLocks.Remove(lockItem);
                await _context.SaveChangesAsync();
                TempData["success"] = "✅ Đã xóa lệnh khóa phòng!";
            }
            return RedirectToAction(nameof(Index));
        }

       
        /// Mở lại hoàn toàn tất cả các phòng trên hệ thống:
        /// 1. Xóa mọi bản ghi lịch khóa đang tồn tại (RoomLocks).
        /// 2. Đặt lại trạng thái tất cả phòng về "Trống/Sẵn sàng" (IsAvailable = true).
        /// 3. Dashboard sẽ cập nhật lại toàn bộ là phòng sẵn sàng đón khách.
    
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteAll()
        {
            // Lấy danh sách toàn bộ lịch khóa và toàn bộ phòng
            var allLocks = await _context.RoomLocks.ToListAsync();
            var allRooms = await _context.Rooms.ToListAsync();

            // Kiểm tra nếu có lịch khóa hoặc có phòng nào đang ở trạng thái 'Đóng'
            if (allLocks.Any() || allRooms.Any(r => !r.IsAvailable))
            {
                // 1. Xóa sạch mọi lịch trình khóa phòng
                if (allLocks.Any())
                {
                    _context.RoomLocks.RemoveRange(allLocks);
                }
                
                // 2. 🔥 Đặt toàn bộ phòng về trạng thái mở (IsAvailable = true)
                foreach (var room in allRooms)
                {
                    room.IsAvailable = true;
                }

                await _context.SaveChangesAsync();
                TempData["success"] = $"✨ Đã mở lại toàn bộ hệ thống phòng và cập nhật Dashboard thành công!";
            }
            else
            {
                TempData["error"] = "Hệ thống hiện không có phòng nào đang ở trạng thái 'Đóng'.";
            }

            var referer = Request.Headers["Referer"].ToString();
            if (!string.IsNullOrEmpty(referer)) return Redirect(referer);

            return RedirectToAction(nameof(Index));
        }
    }
}
