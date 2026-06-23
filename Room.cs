using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace DALTW.Models
{
    /// Thực thể đại diện cho một Phòng (Room) trong khách sạn.
    /// Lưu trữ thông tin cơ bản, trạng thái, và các thuộc tính hỗ trợ hiển thị/upload.
    public class Room
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên phòng không được để trống")]
        [Display(Name = "Tên phòng")]
        public string Name { get; set; }

        ///Giá thuê phòng cơ bản cho mỗi đêm.
        [Range(0, double.MaxValue, ErrorMessage = "Giá phải là số dương")]
        [Display(Name = "Giá / Đêm")]
        public decimal PricePerNight { get; set; }

        /// Mô tả chi tiết về tiện nghi hoặc đặc điểm của phòng. 
        [Display(Name = "Mô tả")]
        public string? Description { get; set; }

        /// Trạng thái hoạt động chính của phòng (Mở/Đóng).
        /// False có nghĩa là phòng đang tạm dừng kinh doanh.
        [Display(Name = "Trạng thái sẵn sàng")]
        public bool IsAvailable { get; set; } = true;

        /// Tình trạng chi tiết của phòng: "Trống", "Đã đặt", "Đang sử dụng", "Đang bảo trì"
        [Display(Name = "Tình trạng phòng")]
        public string Status { get; set; } = "Trống";

        /// Đường dẫn đến ảnh đại diện chính của phòng. 
        public string? ImageUrl { get; set; }

        /// Sức chứa (Số người ở tối đa).
        [Required(ErrorMessage = "Vui lòng chọn số lượng người ở")]
        [Display(Name = "Số người ở")]
        public int Capacity { get; set; } = 2;

        // THUỘC TÍNH BỔ TRỢ (KHÔNG LƯU TRONG DATABASE)
        // Các trường này dùng để tính toán logic hiển thị hoặc nhận dữ liệu từ Form.

        /// Biến tạm: Đánh dấu phòng đã được đặt trong khoảng thời gian đang xem.
        [NotMapped]
        public bool TempIsBooked { get; set; }

        ///  Biến tạm: Kiểm tra phòng có trống trong khoảng thời gian Tìm kiếm của khách không.
        [NotMapped]
        public bool IsAvailableForSearchDates { get; set; } = true;

        /// Biến tạm: Đánh dấu phòng có đang bị Quản trị viên khóa hôm nay hay không. 
        [NotMapped]
        public bool TempIsLockedToday { get; set; }

        // QUAN HỆ (RELATIONSHIP)

        /// ID Tầng liên kết
        [Display(Name = "Tầng")]
        public int? FloorId { get; set; }
        public Floor? Floor { get; set; }

        /// ID loại phòng liên kết. 
        public int RoomTypeId { get; set; }
        
        ///  Tham chiếu đến Loại phòng (Navigation Property).
        public RoomType? RoomType { get; set; }

        ///Danh sách tất cả ảnh chi tiết thuộc về phòng này (Bộ sưu tập ảnh). 
        public List<RoomImage> RoomImages { get; set; } = new();

        // HỖ TRỢ UPLOAD FILE (KHÔNG LƯU TRONG DATABASE)

        ///  Dùng để nhận tệp ảnh đại diện khi người dùng upload từ Form. 
        [NotMapped]
        public IFormFile? ImageFile { get; set; }

        ///  Dùng để nhận danh sách nhiều tệp ảnh cho bộ sưu tập từ Form.
        [NotMapped]
        public List<IFormFile>? ImageFiles { get; set; }
    }
}