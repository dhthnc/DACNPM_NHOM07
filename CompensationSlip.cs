using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DALTW.Models
{
    /// <summary>
    /// Thực thể đại diện cho Phiếu đền bù.
    /// Lập khi khách trả phòng làm hư hỏng hoặc mất mát các tiện nghi được trang bị.
    /// </summary>
    public class CompensationSlip
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Đơn đặt phòng")]
        public int BookingId { get; set; }
        [ForeignKey("BookingId")]
        public Booking? Booking { get; set; }

        [Required]
        [Display(Name = "Tiện nghi bị ảnh hưởng")]
        public int AmenityId { get; set; }
        [ForeignKey("AmenityId")]
        public Amenity? Amenity { get; set; }

        [Required]
        [Display(Name = "Mức độ thiệt hại")]
        public string DamageLevel { get; set; } // Hỏng nhẹ, Hỏng nặng, Mất

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Chi phí đền bù phải lớn hơn hoặc bằng 0")]
        [Display(Name = "Chi phí đền bù")]
        public decimal CompensationCost { get; set; }

        [Required]
        [Display(Name = "Nhân viên lập phiếu")]
        public string EmployeeName { get; set; }

        [Required]
        [Display(Name = "Ngày lập")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Display(Name = "Ghi chú chi tiết")]
        public string? Notes { get; set; }
    }
}
