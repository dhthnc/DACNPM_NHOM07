using System.ComponentModel.DataAnnotations;

namespace DALTW.Models
{
    /// <summary>
    /// Thực thể phân loại các Tiện nghi trong khách sạn (VD: Tivi, Tủ lạnh, Máy nước nóng...).
    /// Định nghĩa đơn giá đền bù mặc định cho mỗi loại.
    /// </summary>
    public class AmenityType
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên loại tiện nghi không được để trống")]
        [Display(Name = "Tên loại tiện nghi")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập giá đền bù mặc định")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá đền bù phải lớn hơn hoặc bằng 0")]
        [Display(Name = "Giá đền bù mặc định")]
        public decimal CompensationPrice { get; set; }

        /// <summary>
        /// Danh sách các thiết bị tiện nghi cụ thể thuộc loại này.
        /// </summary>
        public ICollection<Amenity> Amenities { get; set; } = new List<Amenity>();
    }
}
