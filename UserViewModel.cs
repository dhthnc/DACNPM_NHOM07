using DALTW.Models;

namespace DALTW.Models
{
    /// 
    /// Lớp trung gian (ViewModel) dùng để hiển thị thông tin người dùng trong các trang quản trị.
    /// Giúp kết hợp thông tin cơ bản của User và Vai trò (Role) tương ứng của họ.
    /// 
    public class UserViewModel
    {
        /// <summary> Đối tượng người dùng từ ApplicationUser. </summary>
        public ApplicationUser User { get; set; }

        /// Tên vai trò của người dùng (Ví dụ: Admin, Staff, User). </summary>
        public string Role { get; set; }
    }
}
