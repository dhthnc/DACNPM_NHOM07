using Microsoft.AspNetCore.Identity;

namespace DALTW.Models
{
    /// Thực thể người dùng (User) tùy chỉnh cho ứng dụng.
    /// Kế thừa từ IdentityUser của ASP.NET Core để sử dụng các tính năng có sẵn
    /// như: Tài khoản (Username), Mật khẩu (PasswordHash), Email,...
    public class ApplicationUser : IdentityUser
    {
        /// Họ và tên đầy đủ của người dùng.
        /// Trường này được thêm vào bên cạnh các trường mặc định của Identity.
        public string FullName { get; set; }
    }
}