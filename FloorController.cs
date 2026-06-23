using DALTW.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DALTW.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Staff")]
    public class FloorController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FloorController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =====================================================
        // DANH SÁCH TẦNG + PHÒNG + TRẠNG THÁI PHÒNG
        // =====================================================
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var floors = await _context.Floors

                // Load danh sách phòng thuộc tầng
                .Include(f => f.Rooms)

                    // Load loại phòng
                    .ThenInclude(r => r.RoomType)

                .OrderBy(f => f.Id)
                .ToListAsync();

            return View(floors);
        }

        // =====================================================
        // GET: CREATE
        // =====================================================
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // =====================================================
        // POST: CREATE
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Floor floor)
        {
            try
            {
                // Validate dữ liệu
                if (!ModelState.IsValid)
                {
                    TempData["error"] = "❌ Dữ liệu không hợp lệ!";
                    return View(floor);
                }

                // Kiểm tra trùng tên tầng
                bool isExists = await _context.Floors
                    .AnyAsync(f => f.Name == floor.Name);

                if (isExists)
                {
                    ModelState.AddModelError("", "Tên tầng đã tồn tại!");
                    TempData["error"] = "❌ Tên tầng đã tồn tại!";
                    return View(floor);
                }

                // Thêm tầng
                _context.Floors.Add(floor);

                await _context.SaveChangesAsync();

                TempData["success"] = "✅ Đã thêm tầng mới thành công!";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["error"] = "❌ Có lỗi xảy ra: " + ex.Message;
                return View(floor);
            }
        }

        // =====================================================
        // GET: EDIT
        // =====================================================
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var floor = await _context.Floors
                .FirstOrDefaultAsync(f => f.Id == id);

            if (floor == null)
            {
                TempData["error"] = "❌ Không tìm thấy tầng!";
                return RedirectToAction(nameof(Index));
            }

            return View(floor);
        }

        // =====================================================
        // POST: EDIT
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Floor floor)
        {
            if (id != floor.Id)
            {
                return NotFound();
            }

            try
            {
                if (!ModelState.IsValid)
                {
                    TempData["error"] = "❌ Dữ liệu không hợp lệ!";
                    return View(floor);
                }

                // Kiểm tra tên tầng bị trùng
                bool isDuplicate = await _context.Floors
                    .AnyAsync(f => f.Name == floor.Name && f.Id != floor.Id);

                if (isDuplicate)
                {
                    ModelState.AddModelError("", "Tên tầng đã tồn tại!");
                    TempData["error"] = "❌ Tên tầng đã tồn tại!";
                    return View(floor);
                }

                // Update
                _context.Floors.Update(floor);

                await _context.SaveChangesAsync();

                TempData["success"] = "✅ Cập nhật tầng thành công!";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["error"] = "❌ Lỗi cập nhật: " + ex.Message;

                return View(floor);
            }
        }

        // =====================================================
        // GET: DELETE
        // =====================================================
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var floor = await _context.Floors

                // Load phòng thuộc tầng
                .Include(f => f.Rooms)

                .FirstOrDefaultAsync(f => f.Id == id);

            if (floor == null)
            {
                TempData["error"] = "❌ Không tìm thấy tầng!";
                return RedirectToAction(nameof(Index));
            }

            return View(floor);
        }

        // =====================================================
        // POST: DELETE
        // =====================================================
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var floor = await _context.Floors

                    // Load danh sách phòng
                    .Include(f => f.Rooms)

                    .FirstOrDefaultAsync(f => f.Id == id);

                if (floor == null)
                {
                    TempData["error"] = "❌ Không tìm thấy tầng!";
                    return RedirectToAction(nameof(Index));
                }

                // Không cho xóa nếu còn phòng
                if (floor.Rooms.Any())
                {
                    TempData["error"] =
                        "❌ Không thể xóa tầng này vì vẫn còn phòng thuộc tầng!";

                    return RedirectToAction(nameof(Index));
                }

                // Xóa tầng
                _context.Floors.Remove(floor);

                await _context.SaveChangesAsync();

                TempData["success"] = "✅ Đã xóa tầng thành công!";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["error"] = "❌ Lỗi xóa tầng: " + ex.Message;

                return RedirectToAction(nameof(Index));
            }
        }

        // =====================================================
        // DETAILS
        // =====================================================
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var floor = await _context.Floors

                .Include(f => f.Rooms)
                    .ThenInclude(r => r.RoomType)

                .FirstOrDefaultAsync(f => f.Id == id);

            if (floor == null)
            {
                TempData["error"] = "❌ Không tìm thấy tầng!";
                return RedirectToAction(nameof(Index));
            }

            return View(floor);
        }
    }
}