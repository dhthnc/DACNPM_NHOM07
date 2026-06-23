namespace DALTW.Models
{
    /// ViewModel được sử dụng khi xảy ra lỗi hệ thống (như lỗi Server 500).
    /// Giúp hiển thị ID của yêu cầu (Request ID) để quản trị viên có thể tra cứu Log.
    public class ErrorViewModel
    {
        /// Mã định danh duy nhất của yêu cầu gây ra lỗi. 
        public string? RequestId { get; set; }

        /// Kiểm tra xem có ID yêu cầu hay không để quyết định việc hiển thị trên giao diện.
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
