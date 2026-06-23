using System.ComponentModel.DataAnnotations;

namespace DALTW.Models
{
    /// Thực thể lưu trữ các đường dẫn hình ảnh chi tiết cho Bộ sưu tập (Gallery) của từng phòng.
    /// Cho phép một phòng hiển thị được nhiều góc chụp khác nhau.
    public class RoomImage
    {
        [Key]
        public int Id { get; set; }

        ///  ID của phòng mà ảnh này thuộc về. 
        public int RoomId { get; set; }
        
        ///  Tham chiếu đến Thực thể Phòng (Navigation Property). 
        public Room Room { get; set; }

        ///  Đường dẫn URL (phát sinh từ thư mục wwwroot) của tệp ảnh. 
        [Required]
        public string ImageUrl { get; set; }
    }
}
