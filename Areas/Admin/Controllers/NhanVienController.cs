using BASIC_PROJECT.Models;
using Microsoft.Ajax.Utilities;
using Microsoft.SqlServer.Server;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Data.Entity.Validation;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using System.Xml.Linq;


namespace BASIC_PROJECT.Areas.Admin.Controllers
{
    public class NhanVienController : Controller
    {

        // GET: Admin/NhanVien
        NhaHangEntities3 db = new NhaHangEntities3();
        #region Them_Xoa_Sua
        public ActionResult Index()
        {
            return View(db.NhanViens.ToList().OrderBy(n => n.MaNV));
        }


        public ActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Create(FormCollection f)
        {
            string hoTen = f["HoTen"];
            string sdt = f["SDT"];
            string diaChi = f["DiaChi"];
            string luongCB = f["LuongCB"];
            string ngayBatDauLam = f["Ngaybatdaulam"];
            //  string MaLichlam = "L1";



            // Tạo đối tượng NhanVien và gán dữ liệu từ f
            NhanVien nhanVien = new NhanVien
            {
                HoTen = hoTen,
                SDT = sdt,
                DiaChi = diaChi,
                LuongCB = luongCB,
                Ngaybatdaulam = ngayBatDauLam,
                // MaLichlam = MaLichlam

            };

            if (ModelState.IsValid)
            {

                db.NhanViens.Add(nhanVien);
                try
                {
                    db.SaveChanges();
                    return RedirectToAction("Index");
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




            }
            return View(nhanVien);
        }


        public ActionResult Edit(int id)
        {
            var nv = db.NhanViens.SingleOrDefault(n => n.MaNV == id);
            if (nv == null)
            {
                return HttpNotFound();
            }

            return View(nv);
        }

        [HttpPost]
        public ActionResult Edit(int id, NhanVien nhanVien)
        {
            if (ModelState.IsValid)
            {
                var nv = db.NhanViens.SingleOrDefault(n => n.MaNV == id);
                if (nv == null)
                {
                    return HttpNotFound(); // Xử lý trường hợp nhân viên không tồn tại
                }

                // Cập nhật thông tin của nhân viên từ dữ liệu đầu vào
                nv.HoTen = nhanVien.HoTen;
                nv.SDT = nhanVien.SDT;
                nv.DiaChi = nhanVien.DiaChi;
                nv.LuongCB = nhanVien.LuongCB;
                nv.Ngaybatdaulam = nhanVien.Ngaybatdaulam;

                db.SaveChanges(); // Lưu thay đổi vào cơ sở dữ liệu
                return RedirectToAction("Index");
            }

            return View(nhanVien);
        }



        // Xóa nhân viên __ổn
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            NhanVien nhanVien = db.NhanViens.Find(id);
            if (nhanVien == null)
            {
                return HttpNotFound();
            }

            return View(nhanVien);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            NhanVien nhanVien = db.NhanViens.Find(id);
            db.NhanViens.Remove(nhanVien);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult update(int id)
        {
            var nv = db.NhanViens.SingleOrDefault(n => n.MaNV == id);
            if (nv == null)
            {
                return HttpNotFound();
            }

            return View(nv);
        }

        [HttpPost]
        public ActionResult update(int id, NhanVien nhanVien)
        {
            if (ModelState.IsValid)
            {
                var nv = db.NhanViens.SingleOrDefault(n => n.MaNV == id);
                if (nv == null)
                {
                    return HttpNotFound();
                }

                // Cập nhật thông tin của nhân viên từ dữ liệu đầu vào


                db.SaveChanges(); // Lưu thay đổi vào cơ sở dữ liệu
                return RedirectToAction("Index");
            }

            return View(nhanVien);
        }

        #endregion
        #region sapxeplichlam
        public ActionResult PhanCa()
        {
            return View(db.BangPhanCongs.ToList());
        }
        public JsonResult Copytuantruoc()
        {
            db.Database.ExecuteSqlCommand("EXEC CopyEventsFromLastWeek");
            return Json(new { message = "Dữ liệu tuần sau đã lưu thành công" }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult SaveOrUpdateEmployee(int? id, string title, DateTime ngay, int resourceId, int check)
        {
            try
            {
                var nhanvien = db.NhanViens.FirstOrDefault(e => e.HoTen == title);

                switch (check)
                {
                    case 1:
                        if (id != null)
                        {
                            var lichmoi = new LichLamViec
                            {
                                MaLichLam = Convert.ToInt32(id),
                                MaNhanVien = nhanvien.MaNV,
                                Ngay = ngay,
                                MaCa = resourceId,
                            };
                            db.LichLamViecs.AddOrUpdate(lichmoi);
                        }
                        break;

                    case 2:
                        if (nhanvien != null)
                        {
                            var lichmoi = new LichLamViec
                            {
                                MaNhanVien = nhanvien.MaNV,
                                Ngay = ngay,
                                MaCa = resourceId,
                            };
                            db.LichLamViecs.Add(lichmoi);
                        }
                        else
                        {
                            return Json(new { success = true, message = "Nhân viên không tồn tại" });
                        }
                        break;
                    case 3:
                        if (id != null)
                        {
                            var lichmoi = new LichLamViec
                            {
                                MaLichLam = Convert.ToInt32(id),
                                MaNhanVien = nhanvien.MaNV,
                                Ngay = ngay,
                                MaCa = resourceId,
                            };
                            db.LichLamViecs.Attach(lichmoi);
                            db.Entry(lichmoi).State = EntityState.Deleted;
                        }
                        else
                        {
                            return Json(new { success = true, message = "Nhân viên không tồn tại" });
                        }
                        break;

                    default:
                        return Json(new { success = false, message = "Lỗi đang xảy ra ở đây" });
                }

                db.SaveChanges();

                return Json(new { success = true, message = "Xử lý thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi khi lưu dữ liệu. Vui lòng thử lại sau. " + ex.Message });
            }

        }

        [HttpGet]
        public JsonResult timMaNv(string tenNhanVien)
        {
            var nhanVien = db.NhanViens.FirstOrDefault(nv => nv.HoTen == tenNhanVien);
            int Manv = nhanVien.MaNV;
            return Json(new { MaNV = Manv }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult LuuLichChamCong(List<LichLamViec> lichLamViecData)
        {
            if (ModelState.IsValid)
            {
                foreach (var item in lichLamViecData)
                {

                    var existingRecord = db.LichLamViecs.FirstOrDefault(x => x.MaLichLam == item.MaLichLam);

                    if (existingRecord != null)
                    {
                        var newRecord = new LichLamViec
                        {
                            MaNhanVien = item.MaNhanVien,
                            Ngay = item.Ngay,
                            MaCa = item.MaCa,
                            GioDen = item.GioDen,
                            GioVe = item.GioVe,
                            CoMat = item.CoMat
                        };

                        db.LichLamViecs.Add(newRecord);
                        db.LichLamViecs.Remove(existingRecord);

                        db.SaveChanges();
                    }
                }

             return Json(new { success = true, message = "Lưu dữ liệu thành công" });
            }
            var errorDetails = ModelState.Values.First().Errors.First().ErrorMessage;
            return Json(new { success = false, message = "Dữ liệu không hợp lệ" });
        }


        public ActionResult XepCa()
        {
            return View(db.BangPhanCongs.ToList());
        }
        [HttpPost]

        #endregion

        public ActionResult tinhLuong()
        {
            return View();
        }

        public ActionResult ChamCong()
        {
            return View(db.BangPhanCongs.ToList());
        }









    }
}
