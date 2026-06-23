using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DALTW.Models
{
    /// <summary>
    /// Thực thể đại diện cho Hóa đơn thanh toán của khách hàng khi trả phòng.
    /// </summary>
    public class Invoice
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Đơn đặt phòng")]
        public int BookingId { get; set; }
        [ForeignKey("BookingId")]
        public Booking? Booking { get; set; }

        [Required]
        [Display(Name = "Tiền thuê phòng")]
        public decimal RoomCharge { get; set; }

        [Required]
        [Display(Name = "Tiền sử dụng dịch vụ")]
        public decimal ServiceCharge { get; set; }

        [Required]
        [Display(Name = "Tiền mua hàng")]
        public decimal ProductCharge { get; set; }

        [Required]
        [Display(Name = "Tiền đền bù")]
        public decimal CompensationCharge { get; set; }

        [Required]
        [Display(Name = "Tiền đặt cọc khấu trừ")]
        public decimal DepositDeduction { get; set; }

        [Required]
        [Display(Name = "Tổng tiền thanh toán")]
        public decimal TotalAmount { get; set; } // RoomCharge + ServiceCharge + ProductCharge + CompensationCharge - DepositDeduction

        [Required]
        [Display(Name = "Hình thức thanh toán")]
        public string PaymentMethod { get; set; } // Tiền mặt, Chuyển khoản, Thẻ ngân hàng, Ví điện tử

        [Required]
        [Display(Name = "Ngày thanh toán")]
        public DateTime PaymentDate { get; set; } = DateTime.Now;

        [Required]
        [Display(Name = "Nhân viên thanh toán")]
        public string EmployeeName { get; set; }

        [Required]
        [Display(Name = "Trạng thái thanh toán")]
        public bool IsPaid { get; set; } = false;
    }
}
