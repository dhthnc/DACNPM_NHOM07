using System.ComponentModel.DataAnnotations;

namespace DALTW.Models
{
    /// <summary>
    /// Thực thể đại diện cho Phiếu lắp đặt tiện nghi.
    /// Ghi nhận lịch sử lắp đặt, thay thế, hoặc luân chuyển tiện nghi giữa các phòng.
    /// </summary>
    public class AmenityInstallationSlip
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Số phiếu lắp đặt")]
        public string SlipNumber { get; set; }

        [Required]
        [Display(Name = "Tiện nghi")]
        public int AmenityId { get; set; }
        public Amenity? Amenity { get; set; }

        [Display(Name = "Phòng nhận")]
        public int? RoomId { get; set; }
        public Room? Room { get; set; }

        [Required]
        [Display(Name = "Ngày thực hiện")]
        public DateTime InstallationDate { get; set; } = DateTime.Now;

        [Required]
        [Display(Name = "Tình trạng khi lắp")]
        public string Status { get; set; } = "Tốt";

        [Required]
        [Display(Name = "Nhân viên thực hiện")]
        public string EmployeeName { get; set; }

        [Display(Name = "Ghi chú/Loại tác vụ")]
        public string? Notes { get; set; } // Lắp mới, Thay thế, Luân chuyển
    }
}
