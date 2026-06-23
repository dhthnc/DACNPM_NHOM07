using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DALTW.Models
{
    /// Thực thể trung gian liên kết giữa Đơn đặt phòng và các Dịch vụ đi kèm.
    /// Lưu trữ thông tin khách hàng đã chọn bao nhiêu dịch vụ nào tại thời điểm đặt.
    public class BookingService
    {
        [Key]
        public int Id { get; set; }

        ///  ID của Đơn đặt phòng liên kết.
        public int BookingId { get; set; }
        
        ///  Tham chiếu đến thông tin Đơn đặt phòng (Navigation Property). 
        [ForeignKey("BookingId")]
        public Booking Booking { get; set; }

        ///  ID của Dịch vụ được chọn. 
        public int ServiceId { get; set; }
        
        ///  Tham chiếu đến thông tin Dịch vụ (Navigation Property). 
        [ForeignKey("ServiceId")]
        public Service Service { get; set; }

        ///  Số lượng dịch vụ khách hàng đã yêu cầu. 
        [Required]
        public int Quantity { get; set; }

        ///  Giá của dịch vụ tại thời điểm thanh toán/đặt chỗ. 
        public decimal UnitPrice { get; set; }
    }
}
