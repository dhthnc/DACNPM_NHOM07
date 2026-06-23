using DALTW.Models; // Nạp namespace chứa các Data Model của dự án (Service, ApplicationDbContext...)
using Microsoft.AspNetCore.Authorization; // Nạp thư viện hỗ trợ phân quyền và xác thực người dùng trong ASP.NET Core
using Microsoft.AspNetCore.Mvc; // Nạp namespace chứa các lớp cốt lõi của ASP.NET Core MVC (Controller, IActionResult)
using Microsoft.EntityFrameworkCore; // Nạp Entity Framework Core để tương tác với Cơ sở dữ liệu và truy vấn bất đồng bộ

namespace DALTW.Areas.Admin.Controllers // Định nghĩa namespace để nhóm các Controller thuộc khu vực Admin
{
   
    /// Điều khiển (Controller) quản lý danh mục Dịch vụ đi kèm (như Ăn sáng, Spa, Thuê xe...).
    /// Chỉ dành riêng cho quyền Quản trị cao nhất (Admin).
   
    [Area("Admin")] // Thuộc tính khai báo Controller này thuộc Area tên là "Admin" để phân định URL route
    [Authorize(Roles = "Admin")] // Thuộc tính bảo mật yêu cầu bộ lọc người dùng phải đăng nhập và có Role (Vai trò) là "Admin"
    public class ServiceController : Controller // Khai báo lớp ServiceController kế thừa lớp Controller chung của MVC
    {
        private readonly ApplicationDbContext _context; // Biến private readonly tham chiếu tới DbContext để thao tác với DB
        private readonly IWebHostEnvironment _env; // Biến private readonly chứa các thông tin về môi trường web host hiện tại (tìm đường dẫn vật lý...)

      
        /// Khởi tạo Controller với các dịch vụ cần thiết.
    
        public ServiceController(ApplicationDbContext context, IWebHostEnvironment env) // Hàm khởi tạo (Constructor) nhận các dịch vụ được tiêm thông qua Dependency Injection
        {
            _context = context; // Gán thể hiện DbContext cho biến toàn cục của lớp (_context)
            _env = env; // Gán thể hiện IWebHostEnvironment cho biến toàn cục của lớp (_env)
        }

    
        /// [GET] Hiển thị danh sách tất cả các dịch vụ đang có trong hệ thống.
      
        public async Task<IActionResult> Index() // Định nghĩa Action Index kiểu GET bất đồng bộ, xử lý yêu cầu "/Admin/Service/Index"
        {
            return View(await _context.Services.ToListAsync()); // Lấy toàn bộ bản ghi dịch vụ từ CSDL bằng lệnh bất đồng bộ đưa vào dạng List, sau đó gửi qua View
        }

      
        /// [GET] Trang thêm mới một loại dịch vụ.
    
        public IActionResult Create() // Action xử lý Request ban đầu (GET) để mở Form nhập thông tin dịch vụ mới
        {
            return View(); // Trả về giao diện thêm mới (trang trống) - Views/Service/Create.cshtml
        }

        
        /// [POST] Xử lý lưu dịch vụ mới và upload ảnh đại diện cho dịch vụ.
        
        [HttpPost] // Cờ giới hạn Action này chỉ tiếp nhận HTTP POST request (khi form submit lên từ form Create)
        [ValidateAntiForgeryToken] // Lớp bảo mật Anti-Forgery Token ngăn ngừa hacker làm giả mạo submit request từ trang web bên ngoài (CSRF)
        public async Task<IActionResult> Create(Service service) // Action thu gom dữ liệu POST và rập nó vào bên trong model "Service" rồi hứng bằng biến "service"
        {
            if (ModelState.IsValid) // Filter để kiểm tra thông số property có thoả mãn quy tắc DataAnnotation ở file model Service.cs hay không? 
            {
                // 📂 XỬ LÝ UPLOAD ẢNH DỊCH VỤ
                if (service.ImageFile != null && service.ImageFile.Length > 0) // Kiểm tra nếu thuộc tính ImageFile thực sự đã ngậm một file và dung lượng lớn hơn 0 bit.
                {
                    string wwwRootPath = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"); // Lấy cái Root path cho tập tin Public - "wwwroot"
                    // Tạo tên file ngẫu nhiên để không bị trùng
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(service.ImageFile.FileName); // Trộn cái đuôi file (.jpg, .png) gốc vô mảng chuỗi mã hoá sinh riêng UUID ngẫu nhiên
                    string path = Path.Combine(wwwRootPath, "images", "services"); // Gộp ghép path đi tới vị trí đích "wwwroot/images/services" nhằm lưu hình
                    
                    if (!Directory.Exists(path)) Directory.CreateDirectory(path); // Dòm xét thử file path đấy có thật không, nếu chưa thì sai hệ điều hành lập ra 1 thư mục với thư mục có thật.

                    using (var fileStream = new FileStream(Path.Combine(path, fileName), FileMode.Create)) // Khai báo đối tượng nối dây ghi vào Path vật lí file này theo lệnh mở file chế độ "Create" mới.
                    {
                        await service.ImageFile.CopyToAsync(fileStream); // Viết dữ liệu byte thô của hình trực tiếp xả vô luồng stream để kết thúc quá trình upload tệp
                    }
                    service.ImageUrl = "/images/services/" + fileName; // Đặt một liên kết giả theo kiểu HTTP URL cho string "ImageUrl", tí nữa CSDL sẽ cập nhật dòng này.
                }

                _context.Add(service); // Phím lệnh nói Entity Framework cất đối tượng này thành "Bản Ghi Chờ Ghi Mới".
                await _context.SaveChangesAsync(); // Đóng máy, truyền đi lệnh ExecuteNonQuery với bản cập nhập đó đẩy vào Database (SQL Server), chờ đồng bộ.
                TempData["success"] = "✅ Đã thêm dịch vụ mới thành công!"; // Nạp message báo hỉ báo thành công đẩy ra TempData - cục dữ liệu sống tạm đến qua tab Redirect
                return RedirectToAction(nameof(Index)); // Chuyển hành vi trả lại trang ban đầu View danh sách để xem hàng mới thêm "Index" Action.
            }
            return View(service); // Chặn cuối nhỡ có lỗi từ lúc If Valid sai (bị rỗng trường quan trọng...), vứt cả giao diện View ban đầu dính cả biến kia.
        }

    
        /// [GET] Trang chỉnh sửa thông tin dịch vụ.
       
        public async Task<IActionResult> Edit(int id) // Gọi Http Get từ nút "Edit" ở trang Index để hiện thông báo với dòng chứa id truyền trên tham số
        {
            var service = await _context.Services.FindAsync(id); // Vọc trong Database lấy ra đúng 1 em dịch vụ chứa primary key id
            if (service == null) return NotFound(); // Bỏ chạy ra trang NotFound trang Lỗi (mã 404) vì ngỡ id biến mất hoặc vô lí 

            return View(service); // Khi đã có model đầy đủ, đính nó dô giao diện của Views/Service/Edit.cshtml 
        }

       
        /// [POST] Cập nhật thông tin dịch vụ, xử lý thay thế ảnh cũ nếu có ảnh mới upload.
   
        [HttpPost] // Cũng là Cờ giới hạn hàm cho hành động Nhận Submit khi form sửa được ấn POST
        [ValidateAntiForgeryToken] // Kẹp xác minh an ninh chống mạo danh qua AntiForgeryToken
        public async Task<IActionResult> Edit(int id, Service service) // Nhận đối tượng id nguyên bản nằm ngoài và object tự dệt từ những cục input Data form do user tạo
        {
            if (id != service.Id) return NotFound(); // Ngừa 1 pha User ranh ma đổi Id tệp, ta lấy id truyền trên uri test với Model Id truyền trong Body cho ăn khớp.

            if (ModelState.IsValid) // Xem nó chuẩn format hay chưa bằng filter Validation.
            {
                // Tải bản ghi hiện tại từ DB để so sánh và cập nhật
                var existingService = await _context.Services.FindAsync(id); // Do cái model Service kia được ModelBinder tạo mới bọc id, ta cần gọi Model có sẵn trong Base bằng DbContext trước để rà
                if (existingService == null) return NotFound(); // Bắt quả tang model dưới Base Database rỗng thì xuất báo Lỗi ngay thôi (404) 

                // Gán các thông tin cơ bản
                existingService.Name = service.Name; // Chuyển giao qua Existing Cột Tên thông qua dữ liệu service mới
                existingService.Price = service.Price; // Chuyển giao qua Dữ liệu cột Tiền 
                existingService.Description = service.Description; // Chuyển giao qua Dữ Liệu mô tả
                existingService.IsActive = service.IsActive; // Cứ thế truyền cả luôn Trạng Thái Kích hoạt

                //  XỬ LÝ THAY THẾ ẢNH
                if (service.ImageFile != null && service.ImageFile.Length > 0) // Kiểm soát sự tồn tại nếu có Tự Chọn File ở máy hay không
                {
                    string wwwRootPath = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"); // Yếu Cầu ra đúng thư mục cha Webroot 
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(service.ImageFile.FileName); // Build 1 cái tên mới cho ảnh
                    string folderPath = Path.Combine(wwwRootPath, "images", "services"); // Ghép lại vào folder nhánh ảnh con

                    if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath); // Xem qua thư mục ấy tạo dựng chưa, chưa thì lập ra

                    // Xóa ảnh cũ trên Server nếu nó tồn tại để làm sạch bộ nhớ
                    if (!string.IsNullOrEmpty(existingService.ImageUrl) && existingService.ImageUrl.StartsWith("/images/services/")) // Bắt rễ tìm ảnh hiện tại ở trong Host Máy chủ (phải có đường link folder) 
                    {
                        var oldImagePath = Path.Combine(wwwRootPath, existingService.ImageUrl.TrimStart('/')); // Trim cái chóp phân nhánh ở đầu chữ đi, thành con đường file dẫn thẳng tới File
                        if (System.IO.File.Exists(oldImagePath)) // Bắt cầu dò tìm ảnh trên Window/Linux (có thật tại ổ hay chỉ là ảo)
                        {
                            try { System.IO.File.Delete(oldImagePath); } catch { } // Quét rác xóa tận gỗ ảnh (Nếu vì lý do gì bị chèn/lock dính Lỗi thì lướt qua luôn với block Try-Catch)
                        }
                    }

                    // Lưu file ảnh mới
                    string fullPath = Path.Combine(folderPath, fileName); // Nối đường lại thành chóp file của ảnh mới tinh. 
                    using (var fileStream = new FileStream(fullPath, FileMode.Create)) // Yêu cầu mở nguồn ghi file bằng Stream truyền tải
                    {
                        await service.ImageFile.CopyToAsync(fileStream); // Tuồn data của file vô vùng dòng ghi
                    }

                    existingService.ImageUrl = "/images/services/" + fileName; // Đính lên existingService đường URL path public của model Ảnh kia
                }
                else if (!string.IsNullOrEmpty(service.ImageUrl)) // Xử cho trường hợp mà không tải Ảnh vật dụng lên nhưng cái textBox ImageUrl do ngta điền lại còn nguyên kí tự
                {
                    // Nếu khách nhập đường dẫn ảnh từ bên ngoài (URL), ưu tiên dùng nó
                    existingService.ImageUrl = service.ImageUrl; // Mình đè bẹp cho Model mang tên hình đó đi cho nhẹ máy 
                }

                try
                {
                    _context.Update(existingService); // Trả lại lệnh thay đổi - "Này DBContext tao cập nhật cho thằng Model cũ mày quản nè."
                    await _context.SaveChangesAsync(); // Cắt truyền Query chèn các bảng Database 
                    TempData["success"] = "✅ Cập nhật dịch vụ thành công!"; // Ném lời báo hỉ cập nhập
                    return RedirectToAction(nameof(Index)); // Đảo View list Action Index (Trang chủ)  
                }
                catch (Exception ex) // Rơi vào đây (lỗi hệ thống cơ sở) thì lôi Ex ra khai báo ngầm báo hiệu 
                {
                    TempData["error"] = "❌ Có lỗi xảy ra: " + ex.Message; // Trưng ra cho người dùng coi lỗi bị gì bằng cảnh báo Đỏ. 
                }
            }

            return View(service); // Phòng cho mọi ngã quẹo đều văng View Form (khi failed check ModelState...)
        }

      
        /// [POST] Xóa ảnh đại diện của một dịch vụ.
       
        [HttpPost] // Action xóa ảnh cũng cần Post tránh vô tình ấn bằng URL get 
        [ValidateAntiForgeryToken] // Buộc xác nhận Token gửi kèm tránh mượn hoa dâng Phật xóa nhầm File  
        public async Task<IActionResult> DeleteImage(int id) // Yêu Cầu chỉ lệnh Remove bằng id Truyền qua Endpoint  
        {
            var service = await _context.Services.FindAsync(id); // Lại Truy cập xem ID đó có thuộc ai ở khu Base dữ liệu 
            if (service == null) return NotFound();  // Đuổi khách nếu tìm hoài không ra Id  

            // Xóa file vật lý trên server nếu có
            if (!string.IsNullOrEmpty(service.ImageUrl) && service.ImageUrl.StartsWith("/images/services/")) // Cần xem nếu Model đang giữ thực thể URL nào mà mang dấu server (/images/..) không
            {
                string wwwRootPath = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"); // Dò thư mục public của Web
                var filePath = Path.Combine(wwwRootPath, service.ImageUrl.TrimStart('/')); // Nâng lên đường link tuyệt đối 
                if (System.IO.File.Exists(filePath)) // Gọi System truy xét Tồn tại file gốc
                {
                    try { System.IO.File.Delete(filePath); } catch { } // Xử ngay xoá xóa nhúng lỗi an toàn!
                }
            }

            service.ImageUrl = null; // Biến thuộc tính đại diện web thành vô tận, chả còn hình chụp 
            await _context.SaveChangesAsync(); // Lệnh Entity rủ Db lưu bảng với Column Image URL: Null 

            TempData["success"] = "✅ Đã xóa ảnh của dịch vụ!"; // Thiết lập hộp thoại ghi báo hiệu Xóa vui vẻ   
            return RedirectToAction(nameof(Edit), new { id = service.Id }); // Lặp lại luồng dẫn View "Edit" để user rảnh xem lại giao dĩ vừa trần.
        }

     
        /// [POST] Bật/Tắt trạng thái hoạt động của dịch vụ (Kích hoạt/Hủy kích hoạt).
      
        [HttpPost] // POST cho cờ Action (Thường là gọi API Form Hidden Toggle nút công tắc IsActive) 
        [ValidateAntiForgeryToken] // Phép Token xác thực AntiForge Form để chống hacker Fake Submit Switch này 
        public async Task<IActionResult> Toggle(int id) // Action (Function xử lý đổi đảo IsActive Status)
        {
            var service = await _context.Services.FindAsync(id); // Tra trong Model Service Id này 
            if (service == null) return NotFound(); // Xịt Lỗi không tìm ra  

            service.IsActive = !service.IsActive; // Bắn đảo vị trí Giá Trị Biến Ngược của Bool qua Logic đảo (!true > false)  
            await _context.SaveChangesAsync(); // Cắt nghĩa lệnh gửi qua Cơ Sở Database   
            
            TempData["success"] = $"✅ Đã {(service.IsActive ? "KÍCH HOẠT" : "ẨN")} dịch vụ thành công!"; // String Format mượn TempData nói lời yêu thương.   
            return RedirectToAction(nameof(Index)); // Nhét về Trang chủ lưới cho gọn. 
        }

       
        /// [POST] Xóa vĩnh viễn dịch vụ.
        /// Lưu ý: Chỉ được xóa nếu dịch vụ này CHƯA từng xuất hiện trong bất kỳ đơn đặt phòng nào.
      
        [HttpPost] // Quyết Tâm phải gởi Form để diệt một Database Column (Post Cờ Diệt)  
        [ValidateAntiForgeryToken] // An toàn bằng con mắt CSRF Form  
        public async Task<IActionResult> Delete(int id) // Kéo đối số mang ý đồ tàn khốc là Delete Id  
        {
            var service = await _context.Services.FindAsync(id); // Nhắm tìm Service theo ID do con dao ném  
            if (service != null) // Trường Hợp Đối Tượng Lấy Thành Công Khác Rỗng (Có Entity tồn tại thật) 
            {
                // KIỂM TRA RÀNG BUỘC DỮ LIỆU
                var hasBooking = await _context.BookingServices.AnyAsync(bs => bs.ServiceId == id); // Bắn câu SQL nhanh nhạy "ANY": Tìm cho chúa xem có Bookingservices nào dính khóa của service.ID ko này không? Trả về có hay không luôn (true/false)!   
                if(hasBooking) { // Quyết định rẽ rách rưới do cấm vi phạm ràng buộc Dữ Kiện Database! Có rồi, Đơn Khách đang sử dụng! Không Cho Phá!   
                    TempData["error"] = "⚠️ Dịch vụ này đã được khách hàng đặt trong quá khứ. Bạn không được xóa vĩnh viễn (để giữ lịch sử hóa đơn), hãy dùng chức năng ẨN dịch vụ!"; // Treo Lỗi để chặn Khôn Không bị Data Relate lỗi Conflict!  
                } else { // Xong nhánh 2 nếu như mới tinh tạo hoặc không ai book  
                    _context.Services.Remove(service); // Nhấc cái service lên giỏ Rác (báo cáo là Đối Tượng Xóa) cho ORM theo dõi  
                    await _context.SaveChangesAsync(); // DB Gõ Cục! Commit Xóa hoàn toàn dưới Tầng Data DB!   
                    TempData["success"] = "✅ Đã xóa dịch vụ vĩnh viễn khỏi hệ thống!"; // TempData chải cờ vàng gửi lại view  
                }
            }
            return RedirectToAction(nameof(Index)); //  Tất Toáng về trang Index ngập danh sách. 
        }
    }
}
