using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using BASIC_PROJECT.Models;
using System.Data.Entity;

namespace BASIC_PROJECT.Areas.Admin.Controllers
{
    public class QLMonAn_ComBoController : Controller
    {
        public NhaHangEntities3 db = new NhaHangEntities3();

        // GET: Admin/QLMonAn_ComBo/CreateComBo
        public ActionResult CreateComBo()
        {
            // Lấy danh sách món ăn từ bảng MonAn
            return View();
        }

        // POST: Admin/QLMonAn_ComBo/CreateComBo
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult CreateComBo(FormCollection form)
        {
            if (ModelState.IsValid)
            {
                // Tạo một đối tượng CTCombo từ dữ liệu trong FormCollection
                CTCombo combo = new CTCombo
                {
                    TenComBo = form["TenComBo"],
                    MoTa = form["MoTa"]
                };

                // Thêm combo vào CSDL
                db.CTComboes.Add(combo);
                db.SaveChanges();

                // Thêm món ăn đã chọn vào CTCombo_MonAn

                // Redirect đến trang danh sách combo hoặc trang khác mà bạn muốn
                return RedirectToAction("ListComBo");
            }

            return View();
        }
        public ActionResult ListComBo()
        {
            // Lấy danh sách các tên ComBo không trùng
            var uniqueTenComBoList = db.CTComboes.Select(c => c.TenComBo).Distinct().ToList();

            // Truyền danh sách tên ComBo vào view
            return View(uniqueTenComBoList);
        }
        public class FoodItem
        {
            public int id { get; set; }
            public string TenMon { get; set; }
        }
        public ActionResult Edit(string name)
        {
            if (name == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            CTCombo combo = db.CTComboes.FirstOrDefault(c => c.TenComBo == name);

            if (combo == null)
            {
                return HttpNotFound();
            }

            var maComBo = Convert.ToInt32(combo.MaComBo);
            ViewBag.Combo = combo;
            // Lấy danh sách id và tên món ăn từ bảng Contact dựa vào MaComBo
            var query = from contact in db.Contacts
                        join ctCombo in db.CTComboes on contact.MaComBo equals ctCombo.MaComBo
                        join monAn in db.MonAns on contact.MaMA equals monAn.MaMA
                        where ctCombo.MaComBo == maComBo
                        select new { contact.id, monAn.TenMon };

            List<FoodItem> foodItemsInCombo = query.Select(item => new FoodItem
            {
                id = item.id,
                TenMon = item.TenMon
            }).ToList();

            ViewBag.FoodItemsInCombo = foodItemsInCombo;

            var allFoodItems = db.MonAns.ToList();
            ViewBag.FoodItemsFromMonAn = allFoodItems;

            return View(combo);
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Edit(CTCombo combo, string action)
        {
            if (ModelState.IsValid && action == "Update")
            {
                // Lấy combo cần cập nhật từ CSDL
                var existingCombo = db.CTComboes.FirstOrDefault(c => c.MaComBo == combo.MaComBo);

                if (existingCombo != null)
                {
                    // Cập nhật thông tin TenComBo và MoTa
                    existingCombo.TenComBo = combo.TenComBo;
                    existingCombo.MoTa = combo.MoTa;

                    // Lưu thay đổi vào CSDL
                    db.SaveChanges();

                    return RedirectToAction("ListComBo");
                }
                else
                {
                    return RedirectToAction("ListComBo");
                }
            }

            return View(combo);
        }
        public ActionResult DeleteFoodItem(int id)
        {
            var itemToDelete = db.Contacts.FirstOrDefault(c => c.id == id);
            if (itemToDelete != null)
            {
                db.Contacts.Remove(itemToDelete);
                db.SaveChanges();
            }


            return Redirect("listcombo");
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult AddFoodItem()
        {
            string name = Request.Form["name"];
            if (name == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            CTCombo combo = db.CTComboes.FirstOrDefault(c => c.TenComBo == name);

            if (combo == null)
            {
                return HttpNotFound();
            }

            var maComBo = Convert.ToInt32(combo.MaComBo);
            int MaMA = Convert.ToInt32(Request.Form["MaMA"]);

            // Tạo một đối tượng Contact và thiết lập các giá trị
            Contact contact = new Contact
            {
                MaComBo = maComBo,
                MaMA = MaMA,
                // Các giá trị khác mà bạn muốn thiết lập
            };

            // Thêm contact vào CSDL
            db.Contacts.Add(contact);
            db.SaveChanges();

            return RedirectToAction("ListComBo");
        }
        public ActionResult Deletes(string name)
        {
            if (name == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var combodelete = db.CTComboes.SingleOrDefault(s => s.TenComBo == name);
            db.CTComboes.Remove(combodelete);
            db.SaveChanges();
            return RedirectToAction("Listcombo");
        }
    }
}
