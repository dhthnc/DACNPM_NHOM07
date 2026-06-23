using DALTW.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DALTW.Areas.Admin.Controllers
{
    
    /// Điều khiển (Controller) dành cho Quản trị viên quản lý Danh sách Người dùng.
    /// Bao gồm các chức năg: Xem thông tin, Phân quyền (Admin/Staff/User) và Khóa/Mở khóa tài khoản.
   
    [Area("Admin")]
    [Authorize(Roles = "Admin")] // Bảo mật: Chỉ tài khoản có quyền Admin mới được quản lý người dùng khác
    public class UserController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

       
        /// Khởi tạo Controller với Identity UserManager.
      
        public UserController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

     
        /// [GET] Hiển thị danh sách toàn bộ tài khoản trong hệ thống kèm theo vai trò (Role) tương ứng.
       
        public async Task<IActionResult> Index()
        {
            var users = _userManager.Users.ToList();
            var userViewModels = new List<UserViewModel>();

            foreach (var user in users)
            {
                // Lấy vai trò hiện tại của người dùng (Giả định mỗi người dùng chỉ có 1 vai trò chính)
                var roles = await _userManager.GetRolesAsync(user);
                var role = roles.FirstOrDefault() ?? "User";

                userViewModels.Add(new UserViewModel
                {
                    User = user,
                    Role = role
                });
            }

            return View(userViewModels);
        }

      
        /// [POST/GET] Thực hiện khóa tài khoản người dùng ngay lập tức.
   
        public async Task<IActionResult> Lock(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            // Thiết lập thời gian hết hạn khóa là vô tận (MaxValue)
            user.LockoutEnd = DateTimeOffset.MaxValue;
            await _userManager.UpdateAsync(user);

            TempData["success"] = $"🔒 Đã khóa tài khoản {user.Email}!";
            return RedirectToAction(nameof(Index));
        }

    
        /// [POST/GET] Mở khóa tài khoản cho người dùng.
     
        public async Task<IActionResult> Unlock(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            // Xóa bỏ mốc thời gian khóa
            user.LockoutEnd = null;
            await _userManager.UpdateAsync(user);

            TempData["success"] = $"✨ Đã mở khóa tài khoản {user.Email}!";
            return RedirectToAction(nameof(Index));
        }

      
        /// [POST] Chỉnh sửa quyền hạn (Role) cho người dùng.
        /// Logic: Xóa bỏ mọi quyền cũ và gán quyền mới.
    
        public async Task<IActionResult> SetRole(string id, string role)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            // 1. Lấy và xóa toàn bộ các vai trò hiện có
            var roles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, roles);

            // 2. Gán vai trò mới được chọn
            await _userManager.AddToRoleAsync(user, role);

            TempData["success"] = $"✅ Đã chuyển quyền người dùng sang: {role}";
            return RedirectToAction(nameof(Index));
        }

     
        /// [GET] Xem chi tiết hồ sơ của một người dùng bất kỳ.
   
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault() ?? "User";

            var model = new UserViewModel
            {
                User = user,
                Role = role
            };

            return View(model);
        }
    }
}