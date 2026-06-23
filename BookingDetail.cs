namespace DALTW.Models
{
    /// Thực thể chi tiết cho từng phòng trong một đơn đặt phòng.
    /// Giúp lưu trữ lịch sử giá tại thời điểm đặt và thông tin phòng cụ thể.
    public class BookingDetail
    {
        public int Id { get; set; }

        // =====================================================
        // BOOKING
        // =====================================================

        /// ID của đơn đặt phòng
        public int BookingId { get; set; }

        /// Navigation Property tới Booking
        public Booking? Booking { get; set; }

        // =====================================================
        // ROOM
        // =====================================================

        /// ID phòng được đặt
        public int RoomId { get; set; }

        /// Navigation Property tới Room
        public Room? Room { get; set; }

        // =====================================================
        // PRICE
        // =====================================================

        /// Giá phòng tại thời điểm đặt
        public decimal Price { get; set; }
    }
}