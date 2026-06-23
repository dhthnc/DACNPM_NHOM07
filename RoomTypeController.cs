using DALTW.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DALTW.Areas.Admin.Controllers
{
    /// Điều khiển (Controller) quản lý các Loại phòng (như Standard, VIP, Suite...).
    /// Các loại phòng này được dùng để phân loại và quản lý thuộc tính chung cho các phòng.
    [Area("Admin")]
    [Authorize(Roles = "Admin")] // Bảo mật bậc cao: Chỉ Admin mới có quyền cấu hình loại phòng
    public class RoomTypeController : Controller
    {
        private readonly ApplicationDbContext _context;

        /// Khởi tạo Controller với Database Context.
        public RoomTypeController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// [GET] Hiển thị danh sách toàn bộ các loại phòng hiện có trong hệ thống.
        /// </summary>
        public async Task<IActionResult> Index()
        {
            return View(await _context.RoomTypes.ToListAsync());
        }

        /// <summary>
        /// [GET] Trang tạo mới một loại phòng.
        /// </summary>
        public IActionResult Create()
        {
            return View();
        }

        /// <summary>
        /// [POST] Xử lý lưu loại phòng mới vào cơ sở dữ liệu.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RoomType roomType)
        {
            if (ModelState.IsValid)
            {
                _context.Add(roomType);
                await _context.SaveChangesAsync();
                TempData["success"] = "✅ Đã thêm loại phòng mới thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(roomType);
        }

        /// <summary>
        /// [GET] Trang chỉnh sửa thông tin loại phòng.
        /// </summary>
        public async Task<IActionResult> Edit(int id)
        {
            var roomType = await _context.RoomTypes.FindAsync(id);
            if (roomType == null) return NotFound();

            return View(roomType);
        }

        /// <summary>
        /// [POST] Cập nhật các thay đổi của loại phòng.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, RoomType roomType)
        {
            if (id != roomType.Id) return NotFound();

            if (ModelState.IsValid)
            {
                _context.Update(roomType);
                await _context.SaveChangesAsync();
                TempData["success"] = "✅ Cập nhật loại phòng thành công!";
                return RedirectToAction(nameof(Index));
            }

            return View(roomType);
        }

        /// <summary>
        /// [GET] Trang xác nhận xóa loại phòng.
        /// </summary>
        public async Task<IActionResult> Delete(int id)
        {
            var roomType = await _context.RoomTypes.FindAsync(id);
            if (roomType == null) return NotFound();

            return View(roomType);
        }

        /// <summary>
        /// [POST] Thực hiện xóa loại phòng khỏi hệ thống.
        /// </summary>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var roomType = await _context.RoomTypes.FindAsync(id);
            if (roomType != null)
            {
                _context.RoomTypes.Remove(roomType);
                await _context.SaveChangesAsync();
                TempData["success"] = "✅ Đã xóa loại phòng thành công!";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}