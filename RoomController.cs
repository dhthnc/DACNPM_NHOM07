using DALTW.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace DALTW.Areas.Admin.Controllers
{
    /// Controller quản lý danh sách phòng, thêm/sửa/xóa và trạng thái khóa phòng.
    [Area("Admin")]
    [Authorize(Roles = "Admin,Staff")]
    public class RoomController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public RoomController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }
        /// Hiển thị danh sách phòng cho Admin với các cảnh báo về trạng thái khóa/đã đặt.
      
        public async Task<IActionResult> Index(string keyword, int? roomTypeId, DateTime? checkIn, DateTime? checkOut)
        {
            var query = _context.Rooms
                .Include(r => r.RoomType)
                .Include(r => r.RoomImages)
                .AsQueryable();

            //  FILTER BY ROOM TYPE
            if (roomTypeId.HasValue)
            {
                query = query.Where(r => r.RoomTypeId == roomTypeId.Value);
            }

            // SEARCH
            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(r =>
                    r.Name.Contains(keyword) ||
                    r.RoomType.Name.Contains(keyword));
            }

            var rooms = await query.ToListAsync();
            var today = DateTime.Today;

            // LUÔN kiểm tra xem hôm nay có nằm trong khoảng khóa phòng không
            // Quy tắc: FromDate <= hôm nay < ToDate (ToDate là ngày MỞ LẠI, không tính là ngày khóa)
            // Ví dụ: Khóa từ ngày 3 → ngày 4: chỉ ngày 3 khóa, từ ngày 4 trở đi = đang mở
            var activeLockRoomIds = await _context.RoomLocks
                .Where(l => l.FromDate.Date <= today && l.ToDate.Date > today)
                .Select(l => l.RoomId)
                .Distinct()
                .ToListAsync();

            foreach (var room in rooms)
            {
                room.TempIsLockedToday = activeLockRoomIds.Contains(room.Id);
            }

            // CHECK BOOKING & LOCKS theo khoảng ngày tìm kiếm (CHỈ ĐỂ HIỂN THỊ - KHÔNG FILTER)
            if (checkIn.HasValue && checkOut.HasValue)
            {
                var ci = checkIn.Value;
                var co = checkOut.Value;

                foreach (var room in rooms)
                {
                    bool isBooked = await _context.BookingDetails
                        .Include(b => b.Booking)
                        .AnyAsync(bd =>
                            bd.RoomId == room.Id &&
                            bd.Booking.Status != "Cancelled" &&
                            bd.Booking.CheckIn < co &&
                            bd.Booking.CheckOut > ci
                        );

                    bool isLocked = await _context.RoomLocks.AnyAsync(l =>
                        l.RoomId == room.Id &&
                        l.FromDate < co &&
                        l.ToDate > ci
                    );

                    room.TempIsBooked = isBooked || isLocked;
                }
            }

            ViewBag.RoomTypes = await _context.RoomTypes.ToListAsync();
            ViewBag.SelectedRoomTypeId = roomTypeId;
            ViewBag.Keyword = keyword;
            ViewBag.CheckIn = checkIn?.ToString("yyyy-MM-dd");
            ViewBag.CheckOut = checkOut?.ToString("yyyy-MM-dd");

            return View(rooms);
        }

        // CREATE
      
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            LoadRoomTypes();
            LoadFloors();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(Room room)
        {
            if (!ModelState.IsValid)
            {
                LoadRoomTypes(room.RoomTypeId);
                LoadFloors(room.FloorId);
                return View(room);
            }

            if (room.RoomTypeId == 0)
            {
                ModelState.AddModelError("", "Vui lòng chọn loại phòng!");
                LoadRoomTypes(room.RoomTypeId);
                LoadFloors(room.FloorId);
                return View(room);
            }

            try
            {
                // upload image(s)
                if (room.ImageFiles != null && room.ImageFiles.Count > 0)
                {
                    string webRootPath = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                    string folder = Path.Combine(webRootPath, "images");
                    Directory.CreateDirectory(folder);

                    room.RoomImages = new List<RoomImage>();

                    foreach (var file in room.ImageFiles)
                    {
                        if (file != null && file.Length > 0)
                        {
                            string fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
                            string path = Path.Combine(folder, fileName);

                            using (var stream = new FileStream(path, FileMode.Create))
                            {
                                await file.CopyToAsync(stream);
                            }

                            string url = "/images/" + fileName;

                            if (string.IsNullOrEmpty(room.ImageUrl))
                            {
                                room.ImageUrl = url; // Lưu ảnh đầu tiên làm thumbnail
                            }

                            room.RoomImages.Add(new RoomImage { ImageUrl = url });
                        }
                    }
                }

                _context.Rooms.Add(room);
                await _context.SaveChangesAsync();

                TempData["success"] = "Thêm phòng thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
                LoadRoomTypes(room.RoomTypeId);
                LoadFloors(room.FloorId);
                return View(room);
            }
        }

        // EDIT
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var room = await _context.Rooms
                .Include(r => r.RoomImages)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (room == null) return NotFound();

            LoadRoomTypes(room.RoomTypeId);
            LoadFloors(room.FloorId);
            return View(room);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, Room room)
        {
            if (id != room.Id) return NotFound();

            if (!ModelState.IsValid)
            {
                LoadRoomTypes(room.RoomTypeId);
                LoadFloors(room.FloorId);
                return View(room);
            }

            var existing = await _context.Rooms.FindAsync(id);
            if (existing == null) return NotFound();

            existing.Name = room.Name;
            existing.PricePerNight = room.PricePerNight;
            existing.Description = room.Description;
            existing.RoomTypeId = room.RoomTypeId;
            existing.FloorId = room.FloorId;
            existing.Status = room.Status ?? "Trống";
            existing.Capacity = room.Capacity;
            existing.IsAvailable = room.IsAvailable;

            try
            {
                // update image(s)
                if (room.ImageFiles != null && room.ImageFiles.Count > 0)
                {
                    string webRootPath = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                    string folder = Path.Combine(webRootPath, "images");
                    Directory.CreateDirectory(folder);

                    foreach (var file in room.ImageFiles)
                    {
                        if (file != null && file.Length > 0)
                        {
                            string fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
                            string path = Path.Combine(folder, fileName);

                            using (var stream = new FileStream(path, FileMode.Create))
                            {
                                await file.CopyToAsync(stream);
                            }

                            string url = "/images/" + fileName;

                            // Cập nhật thumbnail nếu phòng chưa có ảnh nào
                            if (string.IsNullOrEmpty(existing.ImageUrl))
                            {
                                existing.ImageUrl = url;
                            }

                            _context.RoomImages.Add(new RoomImage { RoomId = existing.Id, ImageUrl = url });
                        }
                    }
                }

                await _context.SaveChangesAsync();
                TempData["success"] = "Cập nhật thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Lỗi xử lý hệ thống: " + ex.Message);
                LoadRoomTypes(room.RoomTypeId);
                LoadFloors(room.FloorId);
                // Vẫn phải gán lại list ảnh nếu muốn view được chính xác
                room.RoomImages = existing.RoomImages ?? new List<RoomImage>();
                room.ImageUrl = existing.ImageUrl;
                return View(room);
            }
        }
        // DELETE
        public async Task<IActionResult> Delete(int id)
        {
            var room = await _context.Rooms
                .Include(r => r.RoomType)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (room == null) return NotFound();

            return View(room);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var room = await _context.Rooms.FindAsync(id);

            if (room != null)
            {
                _context.Rooms.Remove(room);
                await _context.SaveChangesAsync();
            }

            TempData["success"] = "Xóa thành công!";
            return RedirectToAction(nameof(Index));
        }

        // DETAILS
        public async Task<IActionResult> Details(int id)
        {
            var room = await _context.Rooms
                .Include(r => r.RoomType)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (room == null) return NotFound();

            return View(room);
        }

        // TOGGLE (ĐÓNG / MỞ PHÒNG)
        /// Chuyển đổi trạng thái Đóng/Mở phòng ngay lập tức (Xử lý thủ công).
        public async Task<IActionResult> Toggle(int id)
        {
            // Tìm phòng theo ID
            var room = await _context.Rooms.FindAsync(id);
            if (room == null) return NotFound();

            var today = DateTime.Today;
            // Kiểm tra xem phòng có đang bị khóa bởi lịch trình (RoomLocks) hay không
            var activeLocks = await _context.RoomLocks
                .Where(l => l.RoomId == id && l.FromDate.Date <= today && l.ToDate.Date > today)
                .ToListAsync();

            // Một phòng được coi là đang "Đóng" nếu IsAvailable = false HOẶC có lịch khóa đang hiệu lực
            bool isEffectivelyLocked = !room.IsAvailable || activeLocks.Any();

            if (isEffectivelyLocked)
            {
                // TRƯỜNG HỢP: PHÒNG ĐANG ĐÓNG -> TIẾN HÀNH MỞ LẠI
                // 1. Đặt lại trạng thái chung là có sẵn
                room.IsAvailable = true;
                // 2. Nếu có lịch khóa đang chạy, xóa luôn lịch đó để mở phòng ngay
                if (activeLocks.Any())
                {
                    _context.RoomLocks.RemoveRange(activeLocks);
                }
                await _context.SaveChangesAsync();
                TempData["success"] = "✅ Đã mở phòng thành công!";
                return RedirectToAction(nameof(Index));
            }

            // TRƯỜNG HỢP: PHÒNG ĐANG MỞ -> TIẾN HÀNH ĐÓNG LẠI
            // Chuyển IsAvailable về false để phòng không còn xuất hiện trong kết quả tìm kiếm
            room.IsAvailable = false;
            await _context.SaveChangesAsync();
            TempData["success"] = "🔒 Đã chuyển trạng thái phòng sang 'Đóng'!";
            return RedirectToAction(nameof(Index));
        }

     
        // Action: Xóa một ảnh cụ thể trong Bộ sưu tập (Gallery)
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken] 
        public async Task<IActionResult> DeleteImage(int id) // Đổi tham số thành 'id' để tăng tính tương thích cho Routing
        {
            try
            {
                // 1. Tìm bản ghi ảnh trong Database
                var image = await _context.RoomImages.FindAsync(id);
                if (image == null)
                {
                    TempData["error"] = "⚠️ Không tìm thấy ảnh này trong hệ thống!";
                    return RedirectToAction(nameof(Index)); // Hoặc quay lại Index nếu mất ID
                }

                int roomId = image.RoomId;
                
                // 2. Tải thông tin Phòng có kèm bộ sưu tập ảnh (Include RoomImages)
                // Việc dùng Include() cực kỳ quan trọng để lát nữa EF chọn đúng ảnh đại diện mới
                var room = await _context.Rooms
                    .Include(r => r.RoomImages)
                    .FirstOrDefaultAsync(r => r.Id == roomId);
                
                if (room == null)
                {
                    TempData["error"] = "⚠️ Không tìm thấy phòng tương ứng với ảnh!";
                    return RedirectToAction(nameof(Index));
                }

                // 3. XÓA BẢN GHI DATABASE (table: RoomImages)
                _context.RoomImages.Remove(image);
                
                // 4. XÓA FILE VẬT LÝ TRÊN ĐĨA (thư mục: wwwroot/images)
                if (!string.IsNullOrEmpty(image.ImageUrl))
                {
                    var filePath = Path.Combine(_env.WebRootPath, image.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(filePath))
                    {
                        try { System.IO.File.Delete(filePath); } catch { /* Tạm thời bỏ qua nếu file đang treo */ }
                    }
                }

                // 5. 🔥 CẬP NHẬT ẢNH ĐẠI DIỆN CHÍNH (Thumbnail)
                // Nếu xóa đúng cái ảnh đang làm ảnh bìa cho phòng này
                if (room.ImageUrl == image.ImageUrl)
                {
                    // Lọc ra danh sách ảnh CÒN LẠI (không bao gồm cái vừa xóa)
                    var remainingImage = room.RoomImages.FirstOrDefault(i => i.Id != id);
                    
                    // Nếu vẫn còn ảnh khác -> lấy ngẫu nhiên 1 cái làm ảnh bìa mới
                    // Nếu hết sạch ảnh -> gán null
                    room.ImageUrl = remainingImage?.ImageUrl; 
                }

                // 6. CAM KẾT THAY ĐỔI XUỐNG CƠ SỞ DỮ LIỆU
                await _context.SaveChangesAsync();

                TempData["success"] = "✅ Đã xóa ảnh thành công!";
                
                // Quay lại trang chỉnh sửa đúng Phòng này
                return RedirectToAction(nameof(Edit), new { id = roomId });
            }
            catch (Exception ex)
            {
                // 🔥 BÁO LỖI CHI TIẾT (Nếu có lỗi từ Server)
                TempData["error"] = "❌ Có lỗi xảy ra khi xóa ảnh: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }
        // LOAD ROOM TYPE
        private void LoadRoomTypes(object? selected = null)
        {
            ViewBag.RoomTypes = new SelectList(
                _context.RoomTypes,
                "Id",
                "Name",
                selected
            );
        }

        private void LoadFloors(object? selected = null)
        {
            ViewBag.Floors = new SelectList(
                _context.Floors,
                "Id",
                "Name",
                selected
            );
        }
    }
}