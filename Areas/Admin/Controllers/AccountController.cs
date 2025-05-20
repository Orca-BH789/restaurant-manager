using BASIC_PROJECT_Model.Models;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
namespace BASIC_PROJECT.Areas.Admin.Controllers
{
    public class AccountController : Controller
    {
        // GET: Admin/Account
        public async Task<ActionResult> Index()
        {
            await SupabaseService.InitializeAsync();

            var supabase = SupabaseService.GetClient();
            var Role = await supabase.From<Role>().Get();
            var role = Role.Models;
            ViewBag.roles = role;

            var response = await supabase
                .From<Account>()
                .Select("id, username, role_id, email")
                .Get();

            return View(response?.Models ?? new List<Account>());
        }

        public async Task<ActionResult> Create()
        {
            await SupabaseService.InitializeAsync();
            var supabase = SupabaseService.GetClient();
            var Role = await supabase.From<Role>().Get();
            var role = Role.Models;
            ViewBag.roles = role;
            var empRes = await supabase.From<Employee>().Get();
            var employees = empRes.Models;
            ViewBag.Employees = employees;
            var CusRes = await supabase.From<Customer>().Get();
            var Cus = CusRes.Models;
            ViewBag.Custs = Cus;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(Account account)
        {
            await SupabaseService.InitializeAsync();
            var supabase = SupabaseService.GetClient();

            // Load viewbag data (giữ nguyên code hiện tại)...
            var Role = await supabase.From<Role>().Get();
            var role = Role.Models;
            ViewBag.roles = role;
            var empRes = await supabase.From<Employee>().Get();
            var employees = empRes.Models;
            ViewBag.Employees = employees;
            var CusRes = await supabase.From<Customer>().Get();
            var Cus = CusRes.Models;
            ViewBag.Custs = Cus;

            try
            {
                // Kiểm tra Username trước
                if (string.IsNullOrEmpty(account.Username))
                {
                    ModelState.AddModelError("Username", "Tên đăng nhập không được để trống");
                    return View(account);
                }

                // Kiểm tra RoleId để xử lý đúng CustomerId và EmployeeId
                if (account.RoleId == 4) // Giả sử 4 là role khách hàng
                {
                    // Đối với tài khoản khách hàng, EmployeeId phải là null
                    account.EmployeeId = null;

                    // Kiểm tra CustomerId có hợp lệ không
                    if (!account.CustomerId.HasValue || account.CustomerId <= 0)
                    {
                        ModelState.AddModelError("CustomerId", "Vui lòng chọn khách hàng");
                        return View(account);
                    }
                }
                else
                {
                    // Đối với tài khoản không phải khách hàng, CustomerId phải là null
                    account.CustomerId = null;

                    // Kiểm tra EmployeeId có hợp lệ không
                    if (!account.EmployeeId.HasValue || account.EmployeeId <= 0)
                    {
                        ModelState.AddModelError("EmployeeId", "Vui lòng chọn nhân viên");
                        return View(account);
                    }
                }

                // Tiếp tục thêm tài khoản
                var response = await supabase
                    .From<Account>()
                    .Insert(account);

                if (response.ResponseMessage.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    var content = await response.ResponseMessage.Content.ReadAsStringAsync();
                    ModelState.AddModelError(string.Empty, "Lỗi khi tạo tài khoản: " + content);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Lỗi khi tạo tài khoản: " + ex.Message);
            }

            return View(account);
        }


        public async Task<ActionResult> Edit(int id)
        {
            await SupabaseService.InitializeAsync();
            var supabase = SupabaseService.GetClient();

            var Role = await supabase.From<Role>().Get();
            var role = Role.Models;
            ViewBag.roles = role;
            var empRes = await supabase.From<Employee>().Get();
            var employees = empRes.Models;
            ViewBag.Employees = employees;
            var CusRes = await supabase.From<Customer>().Get();
            var Cus = CusRes.Models;
            ViewBag.Custs = Cus;

            var response = await supabase
                .From<Account>()
                .Filter("id", Supabase.Postgrest.Constants.Operator.Equals, id)
                .Get();

            var account = response.Models.FirstOrDefault();
            if (account == null)
            {
                return HttpNotFound();
            }
            return View(account);
        }

        [HttpPost]
        public async Task<ActionResult> Edit(Account account)
        {
            await SupabaseService.InitializeAsync();
            var supabase = SupabaseService.GetClient();

            var Role = await supabase.From<Role>().Get();
            var role = Role.Models;
            ViewBag.roles = role;
            var empRes = await supabase.From<Employee>().Get();
            var employees = empRes.Models;
            ViewBag.Employees = employees;
            var CusRes = await supabase.From<Customer>().Get();
            var Cus = CusRes.Models;
            ViewBag.Custs = Cus;

            if (ModelState.IsValid)
            {
                try
                {
                    var response = await supabase
                        .From<Account>()
                        .Filter("id", Supabase.Postgrest.Constants.Operator.Equals, account.Id)
                        .Update(account);

                    if (response.ResponseMessage.IsSuccessStatusCode)
                    {
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Failed to update Account");
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, "An error occurred: " + ex.Message);
                }
            }

            return View(account);
        }



        public async Task<ActionResult> Delete(int id)
        {
            await SupabaseService.InitializeAsync();
            var supabase = SupabaseService.GetClient();

            var response = await supabase
                .From<Account>()
                .Filter("Id", Supabase.Postgrest.Constants.Operator.Equals, id)
                .Get();

            var account = response.Models.FirstOrDefault();

            if (account == null)
            {
                return HttpNotFound();
            }

            return View(account);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await SupabaseService.InitializeAsync();
                var supabase = SupabaseService.GetClient();

                // Lấy đối tượng cần xóa
                var getResponse = await supabase
                    .From<Account>()
                    .Filter("id", Supabase.Postgrest.Constants.Operator.Equals, id)
                    .Get();

                var account = getResponse.Models.FirstOrDefault();
                if (account == null)
                    return Json(new { success = false, message = "Không tìm thấy khuyến mãi." });

                // Xóa
                var deleteResponse = await supabase
                    .From<Account>()
                    .Delete(account);

                if (deleteResponse.ResponseMessage.IsSuccessStatusCode)
                    return Json(new { success = true });
                else
                    return Json(new { success = false, message = "Xóa thất bại!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi khi xóa: " + ex.Message });
            }
        }
    }
}