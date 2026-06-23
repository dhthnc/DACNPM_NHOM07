using System.ComponentModel.DataAnnotations;

namespace DALTW.Models
{
    /// <summary>
    /// Thực thể đại diện cho sản phẩm bán hàng tại khách sạn (nước uống, đồ ăn nhanh, quà lưu niệm, vật dụng cá nhân).
    /// </summary>
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên sản phẩm không được để trống")]
        [Display(Name = "Tên sản phẩm")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập đơn vị tính")]
        [Display(Name = "Đơn vị tính")]
        public string Unit { get; set; } // Lon, Chai, Cái, Chiếc...

        [Required(ErrorMessage = "Vui lòng nhập giá bán")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá bán phải lớn hơn hoặc bằng 0")]
        [Display(Name = "Giá bán")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số lượng tồn kho")]
        [Range(0, int.MaxValue, ErrorMessage = "Số lượng tồn kho không được âm")]
        [Display(Name = "Số lượng tồn kho")]
        public int Stock { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn loại sản phẩm")]
        [Display(Name = "Phân loại sản phẩm")]
        public string Category { get; set; } // Nước uống, Đồ ăn nhanh, Quà lưu niệm, Vật dụng cá nhân
    }
}
