using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DALTW.Models
{
    /// <summary>
    /// Thực thể đại diện cho Phiếu bán hàng.
    /// Ghi nhận lịch sử bán sản phẩm và trừ đi tồn kho tương ứng.
    /// </summary>
    public class ProductSaleSlip
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "Đơn đặt phòng liên kết")]
        public int? BookingId { get; set; } // Null nếu bán lẻ thu tiền ngay, có giá trị nếu ghi nợ vào phòng
        [ForeignKey("BookingId")]
        public Booking? Booking { get; set; }

        [Required]
        [Display(Name = "Sản phẩm")]
        public int ProductId { get; set; }
        [ForeignKey("ProductId")]
        public Product? Product { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng mua phải lớn hơn 0")]
        [Display(Name = "Số lượng mua")]
        public int Quantity { get; set; } = 1;

        [Required]
        [Display(Name = "Đơn giá")]
        public decimal UnitPrice { get; set; }

        [Required]
        [Display(Name = "Thành tiền")]
        public decimal TotalAmount { get; set; }

        [Required]
        [Display(Name = "Nhân viên bán hàng")]
        public string EmployeeName { get; set; }

        [Required]
        [Display(Name = "Ngày bán")]
        public DateTime SaleDate { get; set; } = DateTime.Now;
    }
}
