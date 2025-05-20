using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BASIC_PROJECT.Models;
namespace BASIC_PROJECT.Areas.Admin.Controllers
{
    public class FoodController : Controller
    {
        // GET: Admin/Food
        NhaHangEntities3 db = new NhaHangEntities3();
        public ActionResult Index()
        {
            return View(db.MonAns.ToList());
        }
        public ActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Create(MonAn monan, HttpPostedFileBase anh)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (anh != null && anh.ContentLength > 0)
                    {
                        var fileName = Path.GetFileName(anh.FileName);
                        var filePath = Path.Combine(Server.MapPath("~/Images/"), fileName);
                        anh.SaveAs(filePath);
                        monan.anh = fileName;
                    }
                    db.MonAns.Add(monan);
                    db.SaveChanges();

                    // Redirect to the desired page
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    // Log the exception
                    ViewBag.mess = ex.Message;
                    Console.WriteLine(ex.Message);
                }
            }

            return View();
        }


        public ActionResult Edit(int id)
        {
            return View(db.MonAns.Where(s => s.MaMA == id).FirstOrDefault());
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Edit(int id, MonAn monAn)
        {
            if (ModelState.IsValid)
            {
                MonAn existingMonAn = db.MonAns.Find(id);

                if (existingMonAn != null)
                {
                    // Cập nhật thông tin món ăn từ payload
                    existingMonAn.TenMon = monAn.TenMon;
                    existingMonAn.ThontinMonan = monAn.ThontinMonan;
                    existingMonAn.Gia = monAn.Gia;
                    existingMonAn.SLton = monAn.SLton;
                    existingMonAn.DonViTinh = monAn.DonViTinh;
                    existingMonAn.Loai = monAn.Loai;

                    db.SaveChanges();

                    return RedirectToAction("Index"); // Hoặc trả về trang xem chi tiết món ăn.
                }
                else
                {
                    // Mã món ăn không tồn tại
                    return HttpNotFound();
                }
            }

            return View(monAn);
        }
        public ActionResult Delete(int id)
        {
            var food = db.MonAns.SingleOrDefault(f => f.MaMA == id);
            if (food != null)
            {
                db.MonAns.Remove(food);
                db.SaveChanges();
            }
            // Sau khi xóa thành công, bạn có thể chuyển hướng người dùng đến trang "Index" hoặc trang khác.
            return RedirectToAction("Index");
        }



    }
}