using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QLBanPhanMem.Models;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;
using System.Xml.Linq;
using MailKit.Security;
using MimeKit;

namespace QLBanPhanMem.Controllers
{
    public class CartController : Controller
    {
        private readonly AppDbContext _context;

        public int? SOLUONG { get; private set; }

        public CartController(AppDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index(string MyData)
        {
            ViewBag.idspvuaxem = HttpContext.Session.GetString("idspvuaxem");

            if (HttpContext.Session.GetString("uid") == null)
            {
                return RedirectToAction("SignIn", "Account");
            }
            ViewBag.notice = MyData;
            ViewBag.giohang = HttpContext.Session.GetString("dem");
            ViewBag.email = HttpContext.Session.GetString("email");
            ViewBag.uid = HttpContext.Session.GetString("uid");

            string? maTK = HttpContext.Session.GetString("uid");


            var hoadon = await _context.HoaDons
                .FirstOrDefaultAsync(hd => hd.MATK == maTK && hd.TINHTRANG == "Chưa thanh toán");
            if(hoadon==null)
            {
                return View();
            }
            else
            {
                string maHD = hoadon.MAHD;
                ViewBag.tongtien = hoadon.TONGTIEN;
                int dem = 0;
                if (_context.CTHDs != null)
                {
                    var cthd = await _context.CTHDs
                        .Include(p => p.PhanMem)
                        .Where(string.IsNullOrEmpty(maHD) ? p => p.MAHD == maHD : p => p.MAHD == maHD)
                        .ToListAsync();
                    dem = await _context.CTHDs
                   .Where(p => string.IsNullOrEmpty(maHD) || p.MAHD == maHD)
                   .CountAsync();
                    ViewBag.dem = dem;
                    
                    HttpContext.Session.SetString("dem", dem.ToString());
                        
                    return View(cthd);
                }
                
                return View();
            }
                    
        }
        
        public async Task<IActionResult> AddToCart(int productID, int quantity)
        {
            try
            {
                string? maTK = HttpContext.Session.GetString("uid");
                string? maHD = HttpContext.Session.GetString("uid") + DateTime.Now.ToString("ddMMyyyyHHmmss");
                var hoadon = await _context.HoaDons
                    .FirstOrDefaultAsync(hd => hd.MATK == maTK && hd.TINHTRANG == "Chưa thanh toán");
                //Check số lượng key
                var key = await _context.KEYPMs.FirstOrDefaultAsync(k => k.MAPM == productID && k.TINHTRANG == 0);
                if (key == null)
                {
                    
                    ViewBag.MyData = "Xin lỗi, sản phẩm này vừa bán hết 😢.";
                    return RedirectToAction("Details", "Product", new { id= productID, MyData = ViewBag.MyData });
                }
                if (hoadon == null)
                {
                    hoadon = new HoaDonModel
                    {
                        MAHD = maHD,
                        MATK = maTK,
                        THOIGIANLAP = DateTime.Now,
                        TONGTIEN = 0,
                        TINHTRANG = "Chưa thanh toán"
                    };
                    _context.HoaDons.Add(hoadon);
                    await _context.SaveChangesAsync();
                    
                    var cthd = new ChiTietHoaDonModel();
                    cthd = new ChiTietHoaDonModel
                    {
                        MAHD = hoadon.MAHD,
                        MAPM = productID,
                        SOLUONG = quantity,
                        THANHTIEN = (await _context.PhanMems.FirstOrDefaultAsync(pm => pm.MAPM == productID)).DONGIA
                    };
                    _context.CTHDs.Add(cthd);
                    await _context.SaveChangesAsync();
                    int? soluong = cthd.SOLUONG;
                    int? tongtien = (int)(await _context.CTHDs.Where(ct => ct.MAHD == hoadon.MAHD).SumAsync(ct => ct.THANHTIEN)).Value * soluong;
                    hoadon.TONGTIEN = tongtien;
                    _context.Update(hoadon);
                    await _context.SaveChangesAsync();
                }
                else if (hoadon != null)
                {
                    var cthd = await _context.CTHDs.FirstOrDefaultAsync(ct => ct.MAHD == hoadon.MAHD && ct.MAPM == productID);
                    if (cthd != null)
                    {
                        cthd.SOLUONG = cthd.SOLUONG + 1;
                        _context.Update(cthd);
                        await _context.SaveChangesAsync();
                    }
                    else if (cthd == null)
                    {
                        cthd = new ChiTietHoaDonModel
                        {
                            MAHD = hoadon.MAHD,
                            MAPM = productID,
                            SOLUONG = quantity,
                            THANHTIEN = (await _context.PhanMems.FirstOrDefaultAsync(pm => pm.MAPM == productID)).DONGIA
                        };
                        _context.CTHDs.Add(cthd);
                        await _context.SaveChangesAsync();
                    }
                    int? soluong = cthd.SOLUONG;
                    int? tongtien = (int)(await _context.CTHDs.Where(ct => ct.MAHD == hoadon.MAHD).SumAsync(ct => ct.THANHTIEN)).Value * soluong;
                    hoadon.TONGTIEN = tongtien;
                    _context.Update(hoadon);
                    await _context.SaveChangesAsync();
                }
                ViewBag.giohang = HttpContext.Session.GetString("dem");
                //return Json(new 
                //{ 
                //    success = true, 
                //    message = "Sản phẩm đã được thêm vào giỏ hàng" 
                //});
                // return RedirectToAction("Details", "Product", new { id = productID });
                ViewBag.MyData = "Sản phẩm đã được thêm vào giỏ hàng.";
                return RedirectToAction("Details", "Product", new { id = productID, myData = ViewBag.MyData });

            }
            catch (Exception ex)
            {
                // Xử lý ngoại lệ ở đây và tạo đối tượng ProblemDetails
                var problemDetails = new ProblemDetails
                {
                    Title = "Lỗi xử lý yêu cầu",
                    Status = 500, // Hoặc một mã trạng thái HTTP phù hợp khác
                    Detail = ex.Message
                };

                return StatusCode(problemDetails.Status.Value, problemDetails);
            }
        }

        private async void ThemHoaDon(HoaDonModel hoadon, string maHD, string maTK)
        {
            hoadon = new HoaDonModel
            {
                MAHD = maHD,
                MATK = maTK,
                THOIGIANLAP = DateTime.Now,
                TONGTIEN = 0,
                TINHTRANG = "Chưa thanh toán"
            };
            _context.HoaDons.Add(hoadon);
            await _context.SaveChangesAsync();
        }
        private async void ThemCTHD(HoaDonModel hoadon,ChiTietHoaDonModel cthd,int productID)
        {
            cthd = new ChiTietHoaDonModel
            {
                MAHD = hoadon.MAHD,
                MAPM = productID,
                SOLUONG = 1,
                THANHTIEN = (await _context.PhanMems.FirstOrDefaultAsync(pm => pm.MAPM == productID)).DONGIA
            };
            _context.CTHDs.Add(cthd);
            await _context.SaveChangesAsync();
        }
        private async void CapNhatTongTienHD(HoaDonModel hoadon, ChiTietHoaDonModel cthd)
        {
            
            int? soluong = await _context.CTHDs.Where(ct => ct.MAHD == hoadon.MAHD).SumAsync(ct => ct.SOLUONG);
            int? tongtien = (int)(await _context.CTHDs.Where(ct => ct.MAHD == hoadon.MAHD).SumAsync(ct => ct.THANHTIEN)).Value * soluong;
            hoadon.TONGTIEN = tongtien;
            _context.Update(hoadon);
            await _context.SaveChangesAsync();
        }
        private async void addCTHD(int productID, string maHD)
        {
            
            
            //int soluong = (int)cthd.SOLUONG;
            //int tongtien = (int)_context.CTHDs.Where(ct => ct.MAHD == maHD).Sum(ct => ct.THANHTIEN) * soluong;
            //var hoadon = _context.HoaDons.FirstOrDefaultAsync(hd => hd.MAHD == maHD).Result;
            //hoadon.TONGTIEN = tongtien;
            //try
            //{
            //    _context.Update(hoadon);
            //    await _context.SaveChangesAsync();
            //}
            //catch(Exception e)
            //{
            //    Problem("Lỗi cập nhật tổng tiền");
            //}
        }
        //public async Task<IActionResult> AddToCart(int productId)
        //{
        //    string? maTK = HttpContext.Session.GetString("uid");
        //    string? maHD = HttpContext.Session.GetString("uid") + DateTime.Now.ToString("ddMMyyyyHHmmss");
        //    var hoadon = _context.HoaDons
        //    .FirstOrDefault(hd => hd.MATK == maTK && hd.TINHTRANG == "Chưa thanh toán");
        //    if (hoadon==null)
        //    {
        //        var order = new HoaDonModel
        //        {
        //            MAHD = maTK + DateTime.Now.ToString("ddMMyyyyHHmmss"), // Mã hóa đơn là mã khách hàng + thời gian lập
        //            MATK = maTK,
        //            THOIGIANLAP = DateTime.Now,
        //            TONGTIEN = 0, // Ban đầu đặt là 0
        //            TINHTRANG = "Chưa thanh toán"
        //        };
        //        _context.HoaDons.Add(order);
        //        _context.SaveChanges();

        //        var detail = new ChiTietHoaDonModel
        //        {
        //            MAHD = hoadon.MAHD,
        //            MAPM = productId,
        //            SOLUONG = 1,
        //            THANHTIEN = _context.PhanMems
        //                            .FirstOrDefault(pm => pm.MAPM == productId).DONGIA
        //        };
        //        _context.CTHDs.Add(detail);
        //        _context.SaveChanges();
        //        detail = _context.CTHDs.FirstOrDefaultAsync(ct => ct.MAHD == hoadon.MAHD && ct.MAPM == productId).Result;
        //        int soluong = (int)detail.SOLUONG;
        //        int tongtien = (int)_context.CTHDs.Where(ct => ct.MAHD == hoadon.MAHD).Sum(ct => ct.THANHTIEN);
        //        hoadon.TONGTIEN = tongtien;
        //        _context.Update(hoadon);
        //        _context.SaveChanges();
        //    } 
        //    else
        //    {
        //        hoadon = _context.HoaDons.FirstOrDefaultAsync(hd => hd.MATK == maTK && hd.TINHTRANG == "Chưa thanh toán").Result;
        //        string mahd = hoadon.MAHD;
        //        var cthd = _context.CTHDs.FirstOrDefaultAsync(ct => ct.MAHD == mahd && ct.MAPM == productId).Result;
        //        if (cthd != null)
        //        {
        //            cthd.SOLUONG = cthd.SOLUONG + 1;
        //        }
        //        else if (cthd == null)
        //        {
        //            var detail = new ChiTietHoaDonModel
        //            {
        //                MAHD = hoadon.MAHD,
        //                MAPM = productId,
        //                THANHTIEN = _context.PhanMems
        //                            .FirstOrDefault(pm => pm.MAPM == productId).DONGIA
        //            };
        //            _context.CTHDs.Add(detail);
        //        }
        //        int soluong = (int)cthd.SOLUONG;
        //        _context.SaveChanges();
        //        int tongtien = (int)_context.CTHDs.Where(ct => ct.MAHD == hoadon.MAHD).Sum(ct => ct.THANHTIEN)*soluong;
        //        hoadon.TONGTIEN = tongtien;
        //        _context.Update(hoadon);
        //        _context.SaveChanges();
        //    }
        //    int dem = 0;
        //    if (_context.CTHDs != null)
        //    {               
        //        dem = await _context.CTHDs
        //       .Where(p => string.IsNullOrEmpty(maHD) || p.MAHD == maHD)
        //       .CountAsync();               
        //        HttpContext.Session.SetString("dem", dem.ToString());
        //    }
        //    return RedirectToAction("Index", "Cart");
        //}
        public IActionResult Checkout()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> ProcessPayment()
        {
            string maTK = HttpContext.Session.GetString("uid");
            string email = HttpContext.Session.GetString("email");
            var hoadon = await _context.HoaDons
                .FirstOrDefaultAsync(hd => hd.MATK == maTK && hd.TINHTRANG == "Chưa thanh toán");
            
            if (hoadon == null) { 
            
            }
            var hoaDonThanhToan = await _context.HoaDons
                .FirstOrDefaultAsync(hd => hd.MATK == maTK && hd.TINHTRANG == "Chưa thanh toán");

            if (hoaDonThanhToan != null)
            {
                var cthdList = await _context.CTHDs
                    .Where(ct => ct.MAHD == hoaDonThanhToan.MAHD)
                    .ToListAsync(); // Lấy danh sách cthd thay vì dùng FirstOrDefault

                if (cthdList.Any())
                {
                    foreach (var cthd in cthdList)
                    {
                        var key = await _context.KEYPMs.FirstOrDefaultAsync(k => k.MAPM == cthd.MAPM&&k.TINHTRANG==0);
                        if (key != null)
                        {
                            var cthdkey = new CTHDKeyModel()
                            {
                                MAHD = hoaDonThanhToan.MAHD,
                                MAPM = cthd.MAPM,
                                MAKEY = key.MAKEY
                            };

                            await _context.CTHDKeys.AddAsync(cthdkey);
                            //Cap nhat tinh trang key
                            key.TINHTRANG = 1;
                             _context.Entry(key).State = EntityState.Modified;
                        }
                        else
                        {
                            await _context.CTHDs.Include(p => p.PhanMem).Where(ct => ct.MAHD == hoaDonThanhToan.MAHD).ToListAsync();
                            List<string> productsSoldOut = new List<string>();
                            foreach(var ct in cthdList)
                            {
                                productsSoldOut.Add(ct.PhanMem.TENPM);
                            }
                            ViewBag.MyData = "Xin lỗi, sản phẩm "+string.Join(", ", productsSoldOut)+" vừa bán hết 😢.";
                            return RedirectToAction("Index", "Cart", new { MyData = ViewBag.MyData });
                        }
                    }

                    await _context.SaveChangesAsync();
                }

            }

            if(hoadon.TINHTRANG=="Chưa thanh toán")
            {
                hoadon.TINHTRANG = "Đã thanh toán";
                _context.Update(hoadon);
                await _context.SaveChangesAsync();
            }
            var account = await _context.Accounts
                .FirstOrDefaultAsync(tk => tk.Uid == maTK);
            if(account.SurPlus<hoadon.TONGTIEN)
            {
                ViewBag.MyData = "Số dư hiện tại không đủ để thanh toán. <a href=\"Account/TopUp\">Nạp ngay?<a/>";
                 return RedirectToAction("Index", "Cart", new { MyData = ViewBag.MyData });
            }
            else
            {
                account.SurPlus = account.SurPlus - hoadon.TONGTIEN;
                _context.Update(account);
                await _context.SaveChangesAsync();
            }
            var result = new OrderDetailViewModel()
            {
                chiTietHoaDonModel = new List<ChiTietHoaDonModel>(),
                keyPMModel = new List<KEYPMModel>(),
                cthdKeyModel = new List<CTHDKeyModel>()
            };
            var ChiTietHoaDonModel = await _context.CTHDs
                .Where(ct => ct.MAHD == hoadon.MAHD)
                .Include(p => p.PhanMem)
                .ToListAsync();
            var cthdKeyModel = await _context.CTHDKeys
                .Where(hd => hd.MAHD == hoadon.MAHD)
                .Include(p => p.PhanMem)
                .Include(k => k.KEYPM)
                .ToListAsync();
            var keyPMModel = await _context.KEYPMs
                .Where(k => k.TINHTRANG == 1)
                .Include(p => p.PhanMem)
                .ToListAsync();
            result.chiTietHoaDonModel.AddRange(ChiTietHoaDonModel);
            result.keyPMModel.AddRange(keyPMModel);
            result.cthdKeyModel.AddRange(cthdKeyModel);
            //SendEmail(result,email);
              
            ViewBag.maHD = hoadon.MAHD;
           HttpContext.Session.SetString("maHD", hoadon.MAHD);
            return RedirectToAction("PaymentSuccess", "Home");
        }
        private async void SendEmail(OrderDetailViewModel result, string emailTo)
        {
            var email = new MimeMessage();
            email.Sender = new MailboxAddress("UpModern", "quangtrung.nguyen.2016@gmail.com");
            email.From.Add(new MailboxAddress("UpModern", "quangtrung.nguyen.2016@gmail.com"));
            email.To.Add(MailboxAddress.Parse("qtrung1702@outlook.com"));
            email.Subject = "Thanh toán thành công | UpModern";

            

            

            var builder = new BodyBuilder();
            builder.HtmlBody = string.Format("" +
                "<h1>Thanh toán thành công!</h1>" +
                "<p>Cảm ơn bạn đã tin dùng sản phẩm tại UpModern</p>" +
                "<p>Dưới đây là thông tin đăng nhập hoặc khóa kích hoạt cho sản phẩm của bạn</p>");
            builder.HtmlBody += "<table style=\"width:100%\">";
            builder.HtmlBody += "<tr>";
            builder.HtmlBody += "<th>Tên sản phẩm</th>";
            builder.HtmlBody += "<th>Khóa kích hoạt</th>";
            builder.HtmlBody += "<th>Tài khoản</th>";
            builder.HtmlBody += "<th>Mật khẩu</th>";
            builder.HtmlBody += "</tr>";
            foreach (var item in result.cthdKeyModel)
            {
                builder.HtmlBody += "<tr>";
                builder.HtmlBody += "<td>" + item.PhanMem.TENPM + "</td>";
                builder.HtmlBody += "<td>" + item.KEYPM.GIATRI + "</td>";
                builder.HtmlBody += "<td>" + item.KEYPM.TAIKHOAN + "</td>";
                builder.HtmlBody += "<td>" + item.KEYPM.MATKHAU + "</td>";
                builder.HtmlBody += "</tr>";
            }
            email.Body = builder.ToMessageBody();

            // dùng SmtpClient của MailKit
            using var smtp = new MailKit.Net.Smtp.SmtpClient();

            try
            {
                smtp.Connect("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
                smtp.Authenticate("quangtrung.nguyen.2016@gmail.com", "whfq lbmy cbcp vgml");
                await smtp.SendAsync(email);
            }
            catch (Exception ex)
            {
                // Gửi mail thất bại, nội dung email sẽ lưu vào thư mục mailssave
                System.IO.Directory.CreateDirectory("mailssave");
                var emailsavefile = string.Format(@"mailssave/{0}.eml", Guid.NewGuid());
                await email.WriteToAsync(emailsavefile);

                //logger.LogInformation("Lỗi gửi mail, lưu tại - " + emailsavefile);
                // logger.LogError(ex.Message);
            }

            smtp.Disconnect(true);
        }
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            string maTK = HttpContext.Session.GetString("uid");
            var hoadon = _context.HoaDons
                .FirstOrDefault(hd => hd.MATK == maTK && hd.TINHTRANG == "Chưa thanh toán");
            if (hoadon == null)
            {
                return Problem("Null");
            }
            else
            {
                string maHD = hoadon.MAHD;
                var ct = await _context.CTHDs
                    .FirstOrDefaultAsync(ct => ct.MAHD == maHD && ct.MAPM == id);

                if (ct != null)
                {
                    _context.CTHDs.Remove(ct);
                    await _context.SaveChangesAsync();
                }
                //Cập nhật lại giá tiền
                int tongtien = (int)_context.CTHDs.Where(ct => ct.MAHD == hoadon.MAHD).Sum(ct => ct.THANHTIEN);
                hoadon.TONGTIEN = tongtien;
                _context.Update(hoadon);
                await _context.SaveChangesAsync();
                //Thêm key vào cthd
                int dem = 0;
                


                return RedirectToAction("Index", "Cart");
            }
        }
        public IActionResult TopUp()
        {
            return View();
        }
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Delete(int? id)
        //{
        //    if (id == null)
        //    {
        //        return BadRequest();
        //    }

        //    var cthd = await _context.CTHDs.FindAsync(id);
        //    if (cthd == null)
        //    {
        //        return NotFound();
        //    }

        //    _context.CTHDs.Remove(cthd);
        //    await _context.SaveChangesAsync();

        //    return RedirectToAction(nameof(Index));
        //}










    }
}
