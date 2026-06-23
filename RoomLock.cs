using System.ComponentModel.DataAnnotations;

namespace DALTW.Models
{
    /// Thực thể quản lý việc "Khóa phòng" (Locking) trong một khoảng thời gian cụ thể.
    /// Dùng khi khách sạn cần bảo trì phòng hoặc có khách VIP đặt giữ chỗ trước mà không qua website.
    public class RoomLock
    {
        [Key]
        public int Id { get; set; }

        ///  ID của phòng bị khóa.
        public int RoomId { get; set; }
        
        ///Tham chiếu đến Phòng (Navigation Property).
        public Room Room { get; set; }

        ///  Ngày bắt đầu khóa. 
        [Required(ErrorMessage = "Vui lòng chọn ngày bắt đầu")]
        [DataType(DataType.Date)]
        [Display(Name = "Từ ngày")]
        public DateTime FromDate { get; set; }

        /// Ngày kết thúc khóa. 
        [Required(ErrorMessage = "Vui lòng chọn ngày kết thúc")]
        [DataType(DataType.Date)]
        [Display(Name = "Đến ngày")]
        public DateTime ToDate { get; set; }

        ///  Lý do khóa phòng (VD: Sửa ống nước, Sơn lại tường...). 
        [Display(Name = "Lý do khóa")]
        public string? Reason { get; set; }
    }
}
