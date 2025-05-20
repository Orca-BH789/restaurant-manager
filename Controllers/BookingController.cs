using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using System.Web.Mvc;
using System.Data.Entity.Validation; 
using BASIC_PROJECT.Models;
using System.Net;
namespace BASIC_PROJECT.Controllers
{
    public class BookingController : Controller
    {
        // GET: Booking
        NhaHangEntities3 db = new NhaHangEntities3();
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult CreateBooking()
        {
            return View();
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult CreateBooking(FormCollection form)
        {
            try
            {
                if (ModelState.IsValid)
                {
                        Booking booking = new Booking
                        {
                            MaKH = Convert.ToInt32(form["MaKH"]),
                            NgayDat = DateTime.Now,
                            NgayDen = DateTime.Now,
                            Loai = form["Loai"],
                            TongTienGoc = Convert.ToInt32(form["TongTienGoc"]),
                            TrangThai = "Chưa Thanh Toán",
                        };

                        db.Bookings.Add(booking);
                        db.SaveChanges();

                        return RedirectToAction("listbooking");
                    }

            }
            catch (DbEntityValidationException ex)
            {
                // Lấy danh sách các lỗi kiểm tra hợp lệ
                var validationErrors = ex.EntityValidationErrors;

                // Duyệt qua từng thực thể để lấy thông tin lỗi kiểm tra hợp lệ
                foreach (var entityValidationErrors in validationErrors)
                {
                    foreach (var validationError in entityValidationErrors.ValidationErrors)
                    {
                        // Lấy tên trường (property) có lỗi
                        string propertyName = validationError.PropertyName;

                        // Lấy thông báo lỗi
                        string errorMessage = validationError.ErrorMessage;

                        // In thông tin lỗi ra màn hình hoặc lưu vào log
                        System.Diagnostics.Debug.WriteLine($"Lỗi kiểm tra hợp lệ cho trường {propertyName}: {errorMessage}");
                    }
                }
            }
            

            return View();
        }
        public ActionResult CreateBookingOnline()
        {
            return View();
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult CreateBookingOnline(FormCollection form)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    Booking booking = new Booking
                    {
                        MaKH = Convert.ToInt32(form["MaKH"]),
                        NgayDat = Convert.ToDateTime( form["NgayDat"]),
                        NgayDen = Convert.ToDateTime(form["NgayDen"]),
                        Loai = "Online",
                        TongTienGoc = Convert.ToInt32(form["TongTienGoc"]),
                        TrangThai = "Chưa Thanh Toán",
                    };

                    db.Bookings.Add(booking);
                    db.SaveChanges();

                    return RedirectToAction("listbooking");
                }

            }
            catch (DbEntityValidationException ex)
            {
                // Lấy danh sách các lỗi kiểm tra hợp lệ
                var validationErrors = ex.EntityValidationErrors;

                // Duyệt qua từng thực thể để lấy thông tin lỗi kiểm tra hợp lệ
                foreach (var entityValidationErrors in validationErrors)
                {
                    foreach (var validationError in entityValidationErrors.ValidationErrors)
                    {
                        // Lấy tên trường (property) có lỗi
                        string propertyName = validationError.PropertyName;

                        // Lấy thông báo lỗi
                        string errorMessage = validationError.ErrorMessage;

                        // In thông tin lỗi ra màn hình hoặc lưu vào log
                        System.Diagnostics.Debug.WriteLine($"Lỗi kiểm tra hợp lệ cho trường {propertyName}: {errorMessage}");
                    }
                }
            }


            return View();
        }
        public ActionResult Listbooking()
        {
            return View(db.Bookings.ToList().Where(s=>s.TrangThai == "Chưa Thanh Toán"));
        }
        public ActionResult Listtable()
        {
            return View(db.Bans.ToList());
        }
        public ActionResult CreateTable()
        {
            return View();
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult CreateTable(FormCollection form)
        {
            if (ModelState.IsValid)
            {
                Ban ban = new Ban
                {
                    Tenban =form["Tenban"],
                    NgayTao = Convert.ToDateTime(form["NgayTao"]),
                    TrangThai = form["TrangThai"],
                    SoLuongNguoi = Convert.ToInt32(form["SoLuongNguoi"]),
                };

                db.Bans.Add(ban);
                db.SaveChanges();

                return RedirectToAction("listtable");
            }

            return View();
        }
        public ActionResult EditTable(int id)
        {
            return View(db.Bans.Where(n => n.MaBan == id).SingleOrDefault());
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult EditTable(FormCollection form)
        {
            if (ModelState.IsValid)
            {
                int maBan = Convert.ToInt32(form["MaBan"]);
                Ban ban = db.Bans.Find(maBan); // Tìm bản ghi Ban trong cơ sở dữ liệu theo khóa chính MaBan

                if (ban != null)
                {
                    ban.Tenban = form["Tenban"];
                    ban.NgayTao = Convert.ToDateTime(form["NgayTao"]);
                    ban.TrangThai = form["TrangThai"];
                    ban.SoLuongNguoi = Convert.ToInt32(form["SoLuongNguoi"]);

                    // Lưu thay đổi vào cơ sở dữ liệu
                    db.SaveChanges();

                    return RedirectToAction("listtable");
                }
                else
                {
                    return RedirectToAction("listtable");
                }
            }

            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Deletetable(int id)
        {
            Ban ban = db.Bans.SingleOrDefault(x => x.MaBan == id);
            if (ban != null)
            {
                db.Bans.Remove(ban); // Xóa bản ghi
                db.SaveChanges(); // Lưu thay đổi vào cơ sở dữ liệu
            }
            return RedirectToAction("listtable"); // Chuyển hướng đến trang danh sách sau khi xóa
        }
        public ActionResult Edit(int id)
        {
            var model = db.Bookings.Where(n => n.BookID == id).SingleOrDefault();
            return View(model);
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Edit(FormCollection form)
        {
            if (ModelState.IsValid)
            {
                int mabook = Convert.ToInt32(form["BookID"]);
                Booking ban = db.Bookings.Find(mabook); // Tìm bản ghi Ban trong cơ sở dữ liệu theo khóa chính MaBan

                if (ban != null)
                {
                    ban.Loai = form["Loai"];
                    ban.NgayDat = Convert.ToDateTime(form["NgayDat"]);
                    ban.NgayDen = Convert.ToDateTime(form["NgayDen"]);
                    ban.ThoiDiemKetThuc = Convert.ToDateTime(form["ThoiDiemKetThuc"]);
                    ban.TrangThai = form["TrangThai"];
                    ban.TongTienGoc = Convert.ToInt32(form["TongTienGoc"]);

                    // Lưu thay đổi vào cơ sở dữ liệu
                    db.SaveChanges();

                    return RedirectToAction("listbooking");
                }
                else
                {
                    return RedirectToAction("listbooking");
                }
            }

            return View();
        }

        public ActionResult AddMoreDeltailBooking(int id)
        {
            var banList = db.Bans.Where(s=>s.TrangThai=="0").ToList(); // Lấy danh sách tất cả bàn
            var Bookid = Convert.ToInt32(id);
            // Truyền danh sách bàn và BookID vào view sử dụng ViewBag
            ViewBag.BanList = new SelectList(banList, "MaBan", "Tenban", "TrangThai");
            ViewBag.BookID = Bookid;

            return View();
        }
        [AllowHtml]
        public string GhiChu { get; set; }

        [AllowHtml]
        public string NguyenNhan { get; set; }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult AddMoreDeltailBooking(int id, FormCollection f)
        {
            if (ModelState.IsValid)
            {
                int bookID = id;
                int maBan = int.Parse(f["MaBan"]);
                decimal gia = decimal.Parse(f["Gia"]);

                // Tạo một đối tượng DetailBooking và gán dữ liệu từ biểu mẫu
                DetailBooking detailBooking = new DetailBooking
                {
                    BookID = bookID,
                    MaBan = maBan,
                    Gia = gia
                };

                // Lưu đối tượng vào cơ sở dữ liệu
                db.DetailBookings.Add(detailBooking);
                db.SaveChanges();

                // Cập nhật trạng thái của bàn
                var banToUpdate = db.Bans.Find(maBan);
                if (banToUpdate != null)
                {
                    banToUpdate.TrangThai = "1";
                    db.SaveChanges();
                }

                return RedirectToAction("Listbooking");
            }

            ViewBag.BanList = new SelectList(db.Bans, "MaBan", "Tenban");
            return View("Listbooking");
        }

        public ActionResult ViewMoreDeltailBooking()
        {
            var model = from detail in db.DetailBookings
                        join booking in db.Bookings on detail.BookID equals booking.BookID
                        where booking.TrangThai == "Chưa Thanh Toán"
                        select detail;

            return View(model.ToList());
        }
        public ActionResult EditMoreBooking(int id)
        {
            var banList = db.Bans.ToList();
            var Bookdel = db.DetailBookings.Where(n => n.DetaiBookingID == id).ToList();
            var MabanDb = db.DetailBookings.Where(n => n.DetaiBookingID == id).Select(n => n.MaBan).FirstOrDefault(); // Lấy giá trị MaBan từ DetailBooking

            // Tạo một danh sách chọn (SelectList) để hiển thị danh sách tên bàn từ Bans
            SelectList banSelectList = new SelectList(banList, "MaBan", "Tenban", MabanDb); // Chọn giá trị mặc định từ MabanDb

            ViewBag.BanList = banSelectList;

            return View(Bookdel);
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult EditMoreBooking(List<DetailBooking> model)
        {
            if (ModelState.IsValid)
            {
                foreach (var detail in model)
                {
                    var existingDetail = db.DetailBookings.Find(detail.DetaiBookingID);
                    if (existingDetail != null)
                    {
                        existingDetail.MaBan = detail.MaBan;
                        existingDetail.Gia = detail.Gia;

                        // You can also update other properties as needed

                        db.Entry(existingDetail).State = EntityState.Modified;
                    }
                }

                db.SaveChanges();

                return RedirectToAction("ViewMoreDeltailBooking"); // Redirect to the list page or another appropriate page
            }

            // If there are validation errors, return to the same view with the model
            var banList = db.Bans.ToList();
            SelectList banSelectList = new SelectList(banList, "MaBan", "Tenban");
            ViewBag.BanList = banSelectList;

            return View(model);
        }
        public ActionResult AddFoodToDetailBooking(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            DetailBooking detailBooking = db.DetailBookings.Find(id);
            if (detailBooking == null)
            {
                return HttpNotFound();
            }

            // Lấy danh sách món ăn đã được thêm vào DetailBooking
            var foodDetails = db.MonAn_DetailBooking.Where(f => f.DetaiBookingID == id).ToList();
            ViewBag.FoodDetails = foodDetails;

            // Lấy thông tin về GhiChu và NguyenNhan từ bản ghi đầu tiên có DetaiBookingID tương ứng
            var info = db.MonAn_DetailBooking.FirstOrDefault(d => d.DetaiBookingID == id);

            if (info == null)
            {
                info = new MonAn_DetailBooking(); // Create a new instance if info is null
                info.GhiChu = "Vui lòng Add Ghi Chú !";
                info.NguyenNhan = "Vui lòng Add Nguyên Nhân !";
            }
            else
            {
                ViewBag.info = info;
            }

            ViewBag.id = id; 
            // Load danh sách món ăn từ database
            List<MonAn> foodList = db.MonAns.ToList();
            ViewBag.FoodList = foodList;

            return View(detailBooking);
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult AddFoodToDetailBooking(FormCollection form)
        {
            // Lấy dữ liệu từ FormCollection
            string ghiChu = form["GhiChu"];
            string nguyenNhan = form["NguyenNhan"];
            int DetaiBookingID = int.Parse(form["DetaiBookingID"]);

            if (ModelState.IsValid)
            {
                // Lấy danh sách mã món ăn đã chọn từ form
                var selectedFoodIds = form.GetValues("SelectedFoodIds");

                if (selectedFoodIds != null)
                {
                    foreach (var foodId in selectedFoodIds)
                    {
                        if (!string.IsNullOrWhiteSpace(foodId))
                        {
                            int maMA = int.Parse(foodId);

                            // Tạo một bản ghi trong MonAn_DetailBooking
                            MonAn_DetailBooking foodDetail = new MonAn_DetailBooking
                            {
                                MaMA = maMA,
                                DetaiBookingID = DetaiBookingID,
                                GhiChu = ghiChu,
                                NguyenNhan = nguyenNhan,
                                // Thêm các thông tin khác tùy theo yêu cầu
                            };

                            db.MonAn_DetailBooking.Add(foodDetail);
                        }
                    }

                    db.SaveChanges();
                }

                return RedirectToAction("ViewMoreDeltailBooking");
            }

            return View("ViewMoreDeltailBooking");
        }

        public ActionResult RemoveFoodFromDetailBooking(int id)
        {
            // Tìm món ăn chi tiết dựa trên MaMA
            MonAn_DetailBooking foodDetail = db.MonAn_DetailBooking.FirstOrDefault(x => x.MaMA == id);

            if (foodDetail == null)
            {
                // Nếu không tìm thấy món ăn chi tiết, trả về trang ViewMoreDetailBooking hoặc trang lỗi tùy ý.
                return RedirectToAction("ViewMoreDetailBooking");
            }

            // Xóa món ăn chi tiết khỏi cơ sở dữ liệu
            db.MonAn_DetailBooking.Remove(foodDetail);
            db.SaveChanges();

            // Sau khi xóa, trả về trang AddFoodToDetailBooking với DetaiBookingID tương ứng
            return RedirectToAction("AddFoodToDetailBooking", new { id = id });
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult AddTTFoodOrder(FormCollection form)
        {
            int DetaiBookingID = int.Parse(form["DetaiBookingID"]);
            string ghiChu = System.Web.HttpUtility.HtmlEncode(form["GhiChu"]);
            string nguyenNhan = System.Web.HttpUtility.HtmlEncode(form["NguyenNhan"]);

            if (ModelState.IsValid)
            {
                // Cập nhật GhiChu và NguyenNhan trong MonAn_DetailBooking dựa trên DetaiBookingID
                var foodDetails = db.MonAn_DetailBooking.Where(f => f.DetaiBookingID == DetaiBookingID).ToList();
                foreach (var foodDetail in foodDetails)
                {
                    foodDetail.GhiChu = ghiChu;
                    foodDetail.NguyenNhan = nguyenNhan;
                }

                db.SaveChanges();

                return RedirectToAction("ViewMoreDeltailBooking");
            }

            return View("ViewMoreDeltailBooking");
        }

























    }
}