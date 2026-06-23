using System.ComponentModel.DataAnnotations;

namespace DALTW.Models
{
    /// Lớp lưu thông tin đơn đặt phòng
    public class Booking
    {
        [Key]
        public int Id { get; set; }

        // USER
        public string UserId { get; set; } = string.Empty;

        public ApplicationUser? User { get; set; }

        // THỜI GIAN
        [Display(Name = "Ngày nhận phòng")]
        public DateTime CheckIn { get; set; }

        [Display(Name = "Ngày trả phòng")]
        public DateTime CheckOut { get; set; }

        // GIÁ
        public decimal TotalPrice { get; set; }

        public decimal Deposit { get; set; }

        // TRẠNG THÁI
        public string Status { get; set; } = "Pending";

        // THÔNG TIN KHÁCH
        [Required]
        public string CustomerName { get; set; } = string.Empty;

        [Required]
        public string CustomerPhone { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string CustomerEmail { get; set; } = string.Empty;

        public string? PaymentMethod { get; set; }

        public string? CMND_CCCD { get; set; }

        public string? Nationality { get; set; }

        public string? Address { get; set; }

        public string BookingChannel { get; set; } = "Website";

        public int NumberOfGuests { get; set; } = 1;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // RELATIONSHIP
        public List<BookingDetail> BookingDetails { get; set; } = new();

        public List<BookingService> BookingServices { get; set; } = new();
    }
}