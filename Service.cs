using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace DALTW.Models
{
 
    /// Thực thể đại diện cho các dịch vụ tiện ích (Services) mà khách sạn cung cấp.
    /// Ví dụ: Ăn sáng, Giặt là, Đưa đón sân bay, v.v.
   
    public class Service
    {
        [Key]
        public int Id { get; set; }

         ///Tên gọi của dịch vụ. 
        [Required(ErrorMessage = "Vui lòng nhập tên dịch vụ")]
        [Display(Name = "Tên dịch vụ")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập đơn vị tính")]
        [Display(Name = "Đơn vị tính")]
        public string Unit { get; set; } = "Lượt"; // Lượt, Ngày, Giờ...

        /// Mô tả chi tiết về dịch vụ. 
        [Display(Name = "Mô tả")]
        public string? Description { get; set; }

        /// Đơn giá của dịch vụ. 
        [Required(ErrorMessage = "Vui lòng nhập giá dịch vụ")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá phải lớn hơn hoặc bằng 0")]
        [Display(Name = "Giá dịch vụ")]
        public decimal Price { get; set; }

        ///  Đường dẫn ảnh đại diện cho dịch vụ.
        [Display(Name = "Hình ảnh")]
        public string? ImageUrl { get; set; }
        
        ///Thuộc tính hỗ trợ nhận tệp ảnh từ Form (không lưu DB). 
        [NotMapped]
        public IFormFile? ImageFile { get; set; }

        ///  Trạng thái dịch vụ có đang kinh doanh hay không.
        [Display(Name = "Đang hoạt động")]
        public bool IsActive { get; set; } = true;
    }
}
