using DALTW.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DALTW.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Staff")]
    public class RentalManageController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RentalManageController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =====================================================
        // DANH SÁCH PHIẾU THUÊ
        // =====================================================
        public async Task<IActionResult> Index()
        {
            var bookings = await _context.Bookings
                .Include(b => b.BookingDetails)
                    .ThenInclude(d => d.Room)
                        .ThenInclude(r => r.RoomType)
                .Include(b => b.BookingServices)
                    .ThenInclude(bs => bs.Service)
                .Include(b => b.User)
                .OrderByDescending(b => b.Id)
                .ToListAsync();

            return View(bookings);
        }

        // =====================================================
        // CHI TIẾT PHIẾU THUÊ
        // =====================================================
        public async Task<IActionResult> Details(int id)
        {
            var booking = await _context.Bookings
                .Include(b => b.BookingDetails)
                    .ThenInclude(d => d.Room)
                        .ThenInclude(r => r.RoomType)
                .Include(b => b.BookingServices)
                    .ThenInclude(bs => bs.Service)
                .Include(b => b.User)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (booking == null)
            {
                TempData["error"] = "❌ Không tìm thấy phiếu thuê!";
                return RedirectToAction(nameof(Index));
            }

            return View(booking);
        }

        // =====================================================
        // XÁC NHẬN BOOKING
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Confirm(int id)
        {
            var booking = await _context.Bookings
                .Include(b => b.BookingDetails)
                    .ThenInclude(d => d.Room)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (booking == null)
                return NotFound();

            if (booking.Status == "Cancelled" || booking.Status == "Completed")
                return RedirectToAction(nameof(Index));

            booking.Status = "Confirmed";

            // Khóa phòng
            foreach (var detail in booking.BookingDetails)
            {
                if (detail.Room != null)
                {
                    detail.Room.IsAvailable = false;
                    detail.Room.Status = "Đã đặt";
                }
            }

            await _context.SaveChangesAsync();

            TempData["success"] = "✅ Đã xác nhận booking!";
            return RedirectToAction(nameof(Index));
        }

        // =====================================================
        // CHECK IN
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckIn(int id)
        {
            var booking = await _context.Bookings
                .Include(b => b.BookingDetails)
                    .ThenInclude(d => d.Room)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (booking == null)
                return NotFound();

            if (booking.Status != "Confirmed")
                return RedirectToAction(nameof(Index));

            booking.Status = "CheckedIn";

            foreach (var detail in booking.BookingDetails)
            {
                if (detail.Room != null)
                {
                    detail.Room.Status = "Đang sử dụng";
                    detail.Room.IsAvailable = false;
                }
            }

            await _context.SaveChangesAsync();

            TempData["success"] = "✅ Khách đã nhận phòng!";
            return RedirectToAction(nameof(Index));
        }

        // =====================================================
        // LẬP HÓA ĐƠN CHECK-OUT (BƯỚC 1)
        // =====================================================
        [HttpGet]
        public async Task<IActionResult> CheckOutInvoice(int id)
        {
            var booking = await _context.Bookings
                .Include(b => b.BookingDetails)
                    .ThenInclude(d => d.Room)
                        .ThenInclude(r => r.RoomType)
                .Include(b => b.BookingServices)
                    .ThenInclude(bs => bs.Service)
                .Include(b => b.User)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (booking == null)
            {
                TempData["error"] = "❌ Không tìm thấy phiếu thuê!";
                return RedirectToAction(nameof(Index));
            }

            if (booking.Status != "CheckedIn")
            {
                TempData["error"] = "❌ Chỉ lập hóa đơn cho phòng đang ở trạng thái 'Đang sử dụng' (CheckedIn)!";
                return RedirectToAction(nameof(Details), new { id = booking.Id });
            }

            // Tính số đêm lưu trú thực tế (tối thiểu 1 đêm)
            int actualNights = (DateTime.Today - booking.CheckIn.Date).Days;
            if (actualNights <= 0) actualNights = 1;
            ViewBag.ActualNights = actualNights;

            // Tính tiền phòng thực tế
            var room = booking.BookingDetails.FirstOrDefault()?.Room;
            decimal roomCharge = actualNights * (room?.PricePerNight ?? 0);
            ViewBag.RoomCharge = roomCharge;

            // Lấy danh sách dịch vụ và sản phẩm để hiển thị dropdown thêm nhanh
            ViewBag.ServicesList = await _context.Services.Where(s => s.IsActive).ToListAsync();
            ViewBag.ProductsList = await _context.Products.Where(p => p.Stock > 0).ToListAsync();

            // Lấy danh sách thiết bị tiện nghi trong phòng này để làm phần đền bù nếu hỏng hóc
            if (room != null)
            {
                ViewBag.AmenitiesList = await _context.Amenities
                    .Include(a => a.AmenityType)
                    .Where(a => a.RoomId == room.Id && (a.Status == "Tốt" || a.Status == "Mới"))
                    .ToListAsync();
            }
            else
            {
                ViewBag.AmenitiesList = new List<Amenity>();
            }

            return View(booking);
        }

        // =====================================================
        // XỬ LÝ LƯU HÓA ĐƠN & HOÀN TẤT CHECK-OUT (BƯỚC 2)
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessCheckOut(
            int bookingId,
            decimal roomCharge,
            decimal serviceCharge,
            decimal productCharge,
            decimal compensationCharge,
            string paymentMethod,
            List<int> serviceIds,
            List<int> serviceQtys,
            List<int> productIds,
            List<int> productQtys,
            List<int> amenityIds,
            List<string> damageLevels,
            List<decimal> compensationCosts,
            List<string> compensationNotes)
        {
            var booking = await _context.Bookings
                .Include(b => b.BookingDetails)
                    .ThenInclude(d => d.Room)
                .FirstOrDefaultAsync(b => b.Id == bookingId);

            if (booking == null) return NotFound();

            var currentUser = User.Identity?.Name ?? "Nhân viên Lễ tân";

            // 1. Thêm Phiếu Sử Dụng Dịch Vụ (ServiceUsageSlips)
            if (serviceIds != null)
            {
                for (int i = 0; i < serviceIds.Count; i++)
                {
                    var svcId = serviceIds[i];
                    var qty = serviceQtys[i];
                    if (qty <= 0) continue;

                    var service = await _context.Services.FindAsync(svcId);
                    if (service != null)
                    {
                        var slip = new ServiceUsageSlip
                        {
                            BookingId = bookingId,
                            UsageDate = DateTime.Today,
                            ServiceId = svcId,
                            Quantity = qty,
                            UnitPrice = service.Price,
                            TotalAmount = service.Price * qty,
                            EmployeeName = currentUser
                        };
                        _context.ServiceUsageSlips.Add(slip);
                    }
                }
            }

            // 2. Thêm Phiếu Bán Sản Phẩm (ProductSaleSlips) và trừ tồn kho
            if (productIds != null)
            {
                for (int i = 0; i < productIds.Count; i++)
                {
                    var prodId = productIds[i];
                    var qty = productQtys[i];
                    if (qty <= 0) continue;

                    var product = await _context.Products.FindAsync(prodId);
                    if (product != null)
                    {
                        // Giảm tồn kho
                        product.Stock = Math.Max(0, product.Stock - qty);

                        var slip = new ProductSaleSlip
                        {
                            BookingId = bookingId,
                            ProductId = prodId,
                            Quantity = qty,
                            UnitPrice = product.Price,
                            TotalAmount = product.Price * qty,
                            EmployeeName = currentUser,
                            SaleDate = DateTime.Now
                        };
                        _context.ProductSaleSlips.Add(slip);
                    }
                }
            }

            // 3. Thêm Phiếu Đền Bù (CompensationSlips) và cập nhật trạng thái thiết bị
            if (amenityIds != null)
            {
                for (int i = 0; i < amenityIds.Count; i++)
                {
                    var amId = amenityIds[i];
                    var cost = compensationCosts[i];
                    var damage = damageLevels[i];
                    var note = compensationNotes[i];

                    var amenity = await _context.Amenities.FindAsync(amId);
                    if (amenity != null)
                    {
                        // Cập nhật tình trạng tiện nghi
                        amenity.Status = damage; // Ví dụ: Hỏng nhẹ, Hỏng nặng, Mất

                        var slip = new CompensationSlip
                        {
                            BookingId = bookingId,
                            AmenityId = amId,
                            DamageLevel = damage,
                            CompensationCost = cost,
                            EmployeeName = currentUser,
                            CreatedAt = DateTime.Now,
                            Notes = note
                        };
                        _context.CompensationSlips.Add(slip);
                    }
                }
            }

            // 4. Khấu trừ đặt cọc
            decimal depositDeduction = booking.Deposit;
            decimal totalAmount = roomCharge + serviceCharge + productCharge + compensationCharge - depositDeduction;

            // 5. Tạo Hóa Đơn (Invoice)
            var invoice = new Invoice
            {
                BookingId = bookingId,
                RoomCharge = roomCharge,
                ServiceCharge = serviceCharge,
                ProductCharge = productCharge,
                CompensationCharge = compensationCharge,
                DepositDeduction = depositDeduction,
                TotalAmount = totalAmount,
                PaymentMethod = paymentMethod,
                PaymentDate = DateTime.Now,
                EmployeeName = currentUser,
                IsPaid = true
            };
            _context.Invoices.Add(invoice);

            // 6. Cập nhật Booking sang Completed và trả phòng về Trống
            booking.Status = "Completed";
            foreach (var detail in booking.BookingDetails)
            {
                if (detail.Room != null)
                {
                    detail.Room.Status = "Trống";
                    detail.Room.IsAvailable = true;
                }
            }

            await _context.SaveChangesAsync();

            TempData["success"] = "✅ Thanh toán & trả phòng thành công! Đã lập hóa đơn.";
            return RedirectToAction(nameof(InvoiceDetail), new { id = invoice.Id });
        }

        // =====================================================
        // CHI TIẾT HÓA ĐƠN (BƯỚC 3)
        // =====================================================
        [HttpGet]
        public async Task<IActionResult> InvoiceDetail(int id)
        {
            var invoice = await _context.Invoices
                .Include(i => i.Booking)
                    .ThenInclude(b => b.BookingDetails)
                        .ThenInclude(d => d.Room)
                            .ThenInclude(r => r.RoomType)
                .Include(i => i.Booking)
                    .ThenInclude(b => b.User)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (invoice == null) return NotFound();

            // Lấy danh sách dịch vụ và sản phẩm, đền bù phát sinh
            ViewBag.ServiceSlips = await _context.ServiceUsageSlips
                .Include(s => s.Service)
                .Where(s => s.BookingId == invoice.BookingId)
                .ToListAsync();

            ViewBag.ProductSlips = await _context.ProductSaleSlips
                .Include(p => p.Product)
                .Where(p => p.BookingId == invoice.BookingId)
                .ToListAsync();

            ViewBag.CompensationSlips = await _context.CompensationSlips
                .Include(c => c.Amenity)
                    .ThenInclude(a => a.AmenityType)
                .Where(c => c.BookingId == invoice.BookingId)
                .ToListAsync();

            return View(invoice);
        }

        // =====================================================
        // ❌ HỦY BOOKING (MỚI THÊM)
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var booking = await _context.Bookings
                .Include(b => b.BookingDetails)
                    .ThenInclude(d => d.Room)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (booking == null)
                return NotFound();

            if (booking.Status == "Completed")
                return RedirectToAction(nameof(Index));

            booking.Status = "Cancelled";

            foreach (var detail in booking.BookingDetails)
            {
                if (detail.Room != null)
                {
                    detail.Room.Status = "Trống";
                    detail.Room.IsAvailable = true;
                }
            }

            await _context.SaveChangesAsync();

            TempData["success"] = "⚠️ Đã hủy booking thành công!";
            return RedirectToAction(nameof(Index));
        }
    }
}