using System.ComponentModel.DataAnnotations;

namespace DALTW.Models
{
    /// Thực thể dùng để phân loại các loại phòng trong khách sạn.
    /// Ví dụ: Phòng Đơn (Single), Phòng Đôi (Double), Phòng Thương Gia (Deluxe).
    public class RoomType
    {
        [Key]
        public int Id { get; set; }

        /// Tên gọi của loại phòng (VD: VIP Room). 
        [Required(ErrorMessage = "Tên loại phòng không được để trống")]
        [Display(Name = "Tên loại phòng")]
        public string Name { get; set; }

        /// Mô tả về các tiện ích chung của loại phòng này.
        [Display(Name = "Mô tả")]
        public string? Description { get; set; }

        /// 
        /// Danh sách tất cả các thực thể Phòng cụ thể thuộc về loại hình này.
        /// (Mối quan hệ 1-Nhiều: Một loại phòng có thể có nhiều phòng thực tế).
        /// 
        public ICollection<Room> Rooms { get; set; } = new List<Room>();
    }
}