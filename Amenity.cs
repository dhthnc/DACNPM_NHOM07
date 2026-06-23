using System.ComponentModel.DataAnnotations;

namespace DALTW.Models
{
    /// <summary>
    /// Thực thể đại diện cho một Tiện nghi cụ thể (VD: TV phòng 101 với serial cụ thể).
    /// </summary>
    public class Amenity
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Số serial hoặc mã tiện nghi không được để trống")]
        [Display(Name = "Số Serial / Mã số")]
        public string SerialNumber { get; set; }

        [Display(Name = "Loại tiện nghi")]
        public int AmenityTypeId { get; set; }
        public AmenityType? AmenityType { get; set; }

        [Display(Name = "Phòng lắp đặt")]
        public int? RoomId { get; set; }
        public Room? Room { get; set; }

        [Required]
        [Display(Name = "Tình trạng")]
        public string Status { get; set; } = "Tốt"; // Mới, Tốt, Hỏng nhẹ, Hỏng nặng, Mất
    }
}
