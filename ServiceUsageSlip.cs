using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DALTW.Models
{
    /// <summary>
    /// Thực thể đại diện cho Phiếu sử dụng dịch vụ.
    /// Ghi nhận mỗi lần khách sử dụng dịch vụ và tự động cộng dồn nếu sử dụng cùng một dịch vụ trong cùng một ngày.
    /// </summary>
    public class ServiceUsageSlip
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Mã đơn đặt phòng")]
        public int BookingId { get; set; }
        [ForeignKey("BookingId")]
        public Booking? Booking { get; set; }

        [Required]
        [Display(Name = "Ngày sử dụng")]
        public DateTime UsageDate { get; set; } = DateTime.Today;

        [Required]
        [Display(Name = "Dịch vụ")]
        public int ServiceId { get; set; }
        [ForeignKey("ServiceId")]
        public Service? Service { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0")]
        [Display(Name = "Số lượng")]
        public int Quantity { get; set; } = 1;

        [Required]
        [Display(Name = "Đơn giá")]
        public decimal UnitPrice { get; set; }

        [Required]
        [Display(Name = "Thành tiền")]
        public decimal TotalAmount { get; set; }

        [Required]
        [Display(Name = "Nhân viên lập phiếu")]
        public string EmployeeName { get; set; }
    }
}
