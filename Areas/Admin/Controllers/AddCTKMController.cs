using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BASIC_PROJECT.Models;

namespace BASIC_PROJECT.Areas.Admin.Controllers
{
    public class AddCTKMController : Controller
    {
        NhaHangEntities3 db = new NhaHangEntities3();
        public class MonAnSearchResult
        {
            public int MaMA { get; set; }
            public string TenMon { get; set; }
            public string anh { get; set; }
        }

        public class AddKhuyenMaiViewModel
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public List<MonAn> MonAns { get; set; }
            public List<CTKhuyenMaiViewModel> CTKhuyenMais { get; set; }
        }

        public class CTKhuyenMaiViewModel
        {
            public int MaMA { get; set; }
            public decimal Gia { get; set; } // Make sure this property is defined
            public string TenMon { get; set; }
            public string anh { get; set; }
        }


        public ActionResult AddKhuyenMai(int id, string name, string search)
        {
            // Lấy danh sách idmonan từ CTKhuyenMais
            var idmonan = db.CTKhuyenMais.Select(s => s.MaMA).ToList();

            // Lấy danh sách Món ăn từ cơ sở dữ liệu, loại trừ các mục có MaMA trùng với idmonan
            var MonAns = db.MonAns.Where(s => !idmonan.Contains(s.MaMA)).ToList();

            // Apply search filter if the search parameter is provided
            if (!string.IsNullOrEmpty(search))
            {
                // Sử dụng câu truy vấn SQL để tìm kiếm trong cơ sở dữ liệu
                MonAns = SearchMonAn(search);
            }

            var query = "SELECT dbo.CTKhuyenMai.MaMA, dbo.MonAn.TenMon, dbo.MonAn.anh, dbo.MonAn.Gia" +
            " FROM dbo.CTKhuyenMai " +
            " INNER JOIN dbo.KhuyenMai ON dbo.CTKhuyenMai.MaKM = dbo.KhuyenMai.MaKM " +
            " INNER JOIN dbo.MonAn ON dbo.CTKhuyenMai.MaMA = dbo.MonAn.MaMA " +
            " WHERE (dbo.KhuyenMai.MaKM = @id)";


            var parameters = new SqlParameter("id", id);
            var CTKhuyenMais = db.Database.SqlQuery<CTKhuyenMaiViewModel>(query, parameters).ToList();

            var viewModel = new AddKhuyenMaiViewModel
            {
                Id = id,
                Name = name,
                MonAns = MonAns,
                CTKhuyenMais = CTKhuyenMais
            };

            return View(viewModel);
        }

        private List<MonAn> SearchMonAn(string search)
        {
            var searchQuery = "SELECT MaMA, TenMon, anh FROM dbo.MonAn WHERE TenMon LIKE @search";
            var searchParameters = new SqlParameter("search", "%" + search + "%");
            var searchResults = db.Database.SqlQuery<MonAnSearchResult>(searchQuery, searchParameters).ToList();

            return searchResults.Select(result => new MonAn
            {
                MaMA = result.MaMA,
                TenMon = result.TenMon,
                anh = result.anh
            }).ToList();
        }

        [HttpPost]
        public ActionResult AddMonAnToCTKhuyenMai(int MaMA, int MaKM, string name)
        {
            if (MaKM > 0 && MaMA > 0)
            {
                // Kiểm tra xem MaMA đã tồn tại trong CTKhuyenMai chưa
                var existingCTKhuyenMai = db.CTKhuyenMais.FirstOrDefault(ct => ct.MaKM == MaKM && ct.MaMA == MaMA);

                if (existingCTKhuyenMai == null)
                {
                    // Nếu MaMA chưa tồn tại, thêm nó vào CTKhuyenMai
                    var newCTKhuyenMai = new CTKhuyenMai
                    {
                        MaKM = MaKM,
                        MaMA = MaMA
                    };

                    db.CTKhuyenMais.Add(newCTKhuyenMai);
                    db.SaveChanges();
                }
            }

            // Chuyển hướng trở lại trang AddKhuyenMai sau khi thực hiện xong
            return RedirectToAction("AddKhuyenMai", new { id = MaKM, name = name, search = "" });
        }

        [HttpPost]
        public ActionResult DeleteMonAnFromCTKhuyenMai(int MaMA, int MaKM, string name)
        {
            if (MaKM > 0 && MaMA > 0)
            {
                // Kiểm tra xem MaMA đã tồn tại trong CTKhuyenMai chưa
                var existingCTKhuyenMai = db.CTKhuyenMais.FirstOrDefault(ct => ct.MaKM == MaKM && ct.MaMA == MaMA);

                if (existingCTKhuyenMai != null)
                {
                    // Nếu MaMA tồn tại, xóa nó khỏi CTKhuyenMai
                    db.CTKhuyenMais.Remove(existingCTKhuyenMai);
                    db.SaveChanges();
                }
            }

            // Chuyển hướng trở lại trang gốc sau khi thực hiện xong
            return RedirectToAction("AddKhuyenMai", new { id = MaKM, name = name });
        }

    }
}
