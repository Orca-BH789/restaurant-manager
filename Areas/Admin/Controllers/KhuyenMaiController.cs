using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity.Validation;
using System.Data.Entity;

using BASIC_PROJECT.Models;

namespace BASIC_PROJECT.Areas.Admin.Controllers
{
    public class KhuyenMaiController : Controller
    {
        // GET: Admin/KhuyenMai
        NhaHangEntities3 db = new NhaHangEntities3();

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Create()
        {
            return View();
        }

        public class KhuyenMaiViewModel
        {
            public string TenKhuyenMai { get; set; }
            public string MoTa { get; set; }
            public DateTime NgayBatDau { get; set; }
            public DateTime NgayKetThuc { get; set; }
            public decimal GiamTien { get; set; }
            public double GiamPT { get; set; }
            public int SLMin { get; set; }
        }

        [HttpPost]
        public ActionResult Create(KhuyenMai khuyenMai)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Kiểm tra tính hợp lệ của ngày bắt đầu và ngày kết thúc
                    if (khuyenMai.NgayBD > khuyenMai.NgayKT)
                    {
                        ModelState.AddModelError(string.Empty, "Ngày bắt đầu không thể lớn hơn ngày kết thúc.");
                        return View(khuyenMai);
                    }

                    // Kiểm tra tính hợp lệ của các trường khác tùy theo quy định

                    if (khuyenMai.GiamGiaTheoTien < 0)
                    {
                        ModelState.AddModelError(string.Empty, "Giảm giá tiền không thể là số âm.");
                        return View(khuyenMai);
                    }
                    if (khuyenMai.SoLuongMuaToiThieu > 1)
                    {
                        ModelState.AddModelError(string.Empty, "Số lượng mua tối thiếu phải lớn hơn 1");
                        return View(khuyenMai);
                    }
                    if (khuyenMai.GiamGiaPhanTram < 0 || khuyenMai.GiamGiaPhanTram > 1)
                    {
                        ModelState.AddModelError(string.Empty, "Giảm giá phần trăm phải nằm trong khoảng từ 0 đến 1.");
                        return View(khuyenMai);
                    }
                    if (khuyenMai.GiamGiaPhanTram is null || khuyenMai.GiamGiaPhanTram is null || khuyenMai.TenKM is null || khuyenMai.Thongtin_CTKM is null || khuyenMai.SoLuongMuaToiThieu is null)
                    {
                        ModelState.AddModelError(string.Empty, "Vui lòng nhập đầy đủ thông tin.");
                        return View(khuyenMai);
                    }
                    // Tạo một đối tượng KhuyenMai từ dữ liệu trong KhuyenMaiViewModel
                    KhuyenMai km = new KhuyenMai
                    {
                        TenKM = khuyenMai.TenKM,
                        Thongtin_CTKM = khuyenMai.Thongtin_CTKM,
                        NgayBD = khuyenMai.NgayBD,
                        NgayKT = khuyenMai.NgayKT,
                        GiamGiaTheoTien = khuyenMai.GiamGiaTheoTien,
                        GiamGiaPhanTram = khuyenMai.GiamGiaPhanTram,
                        SoLuongMuaToiThieu = khuyenMai.SoLuongMuaToiThieu
                    };

                    db.KhuyenMais.Add(km);
                    db.SaveChanges();

                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, "An error occurred: " + ex.Message);
                }
            }

            return View(khuyenMai);
        }

        public ActionResult ListKhuyenMai()
        {
            return View(db.KhuyenMais.ToList());
        }
        public ActionResult Edit(int id)
        {
            var khuyenMai = db.KhuyenMais.FirstOrDefault(k => k.MaKM == id);
            if (khuyenMai == null)
            {
                return HttpNotFound();
            }

            return View(khuyenMai);
        }
        [HttpPost]
        public ActionResult Edit(KhuyenMai khuyenMai)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Trạng thái của đối tượng đang được theo dõi bởi Entity Framework đã thay đổi.
                    db.Entry(khuyenMai).State = EntityState.Modified;
                    db.SaveChanges();

                    return RedirectToAction("ListKhuyenMai");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, "An error occurred: " + ex.Message);
                }
            }

            // Nếu có lỗi hoặc dữ liệu không hợp lệ, hiển thị view `Edit` với dữ liệu nguyên gốc.
            return View(khuyenMai);
        }
        public ActionResult Delete(int id)
        {
            // Truy vấn đối tượng KhuyenMai dựa trên MaKM
            KhuyenMai khuyenMai = db.KhuyenMais.Find(id);

            if (khuyenMai == null)
            {
                // Nếu không tìm thấy đối tượng, hiển thị lỗi hoặc chuyển hướng tùy ý.
                return HttpNotFound();
            }

            return View(khuyenMai);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            KhuyenMai khuyenMai = db.KhuyenMais.Find(id);

            if (khuyenMai == null)
            {
                return HttpNotFound();
            }

            db.KhuyenMais.Remove(khuyenMai);
            db.SaveChanges();

            return RedirectToAction("ListKhuyenMai");
        }

    }

}