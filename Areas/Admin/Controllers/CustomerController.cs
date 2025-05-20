using BASIC_PROJECT_Model.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace BASIC_PROJECT.Areas.Admin.Controllers
{
    public class CustomerController : Controller
    {
        // GET: Admin/Customer
        public async Task<ActionResult> Index()
        {
            await SupabaseService.InitializeAsync();
            var supabase = SupabaseService.GetClient();

            var response = await supabase
                .From<Customer>()
                .Select("customer_id, full_name, email, phone_number ")
                .Get();

            return View(response?.Models ?? new List<Customer>());
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Create(Customer customer)
        {
            if (string.IsNullOrEmpty(customer.FullName) ||
                string.IsNullOrEmpty(customer.Email) ||
                string.IsNullOrEmpty(customer.PhoneNumber))

            {
                ModelState.AddModelError(string.Empty, "Vui lòng điền đầy đủ thông tin");
            }


            if (ModelState.IsValid)
            {
                try
                {
                    await SupabaseService.InitializeAsync();
                    var supabase = SupabaseService.GetClient();

                    var response = await supabase
                        .From<Customer>()
                        .Insert(customer);

                    if (response.ResponseMessage.IsSuccessStatusCode)
                    {
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Thêm khách hàng thất bại.");
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, "Lỗi khi thêm khách hàng: " + ex.Message);
                }
            }

            return View(customer);
        }

        public async Task<ActionResult> Edit(int id)
        {
            await SupabaseService.InitializeAsync();
            var supabase = SupabaseService.GetClient();

            var response = await supabase
                .From<Customer>()
                .Filter("customer_id", Supabase.Postgrest.Constants.Operator.Equals, id)
                .Get();

            var customer = response.Models.FirstOrDefault();

            if (customer == null)
                return HttpNotFound();

            return View(customer);
        }

        [HttpPost]
        public async Task<ActionResult> Edit(Customer customer)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await SupabaseService.InitializeAsync();
                    var supabase = SupabaseService.GetClient();

                    var response = await supabase
                        .From<Customer>()
                        .Filter("customer_id", Supabase.Postgrest.Constants.Operator.Equals, customer.CustomerId)
                        .Update(customer);

                    if (response.ResponseMessage.IsSuccessStatusCode)
                    {
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Cập nhật nhân viên thất bại.");
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, "Lỗi khi cập nhật khách hàng: " + ex.Message);
                }
            }

            return View(customer);
        }

        public async Task<ActionResult> Delete(int id)
        {
            await SupabaseService.InitializeAsync();
            var supabase = SupabaseService.GetClient();

            var response = await supabase
                .From<Customer>()
                .Filter("customer_id", Supabase.Postgrest.Constants.Operator.Equals, id)
                .Get();

            var customer = response.Models.FirstOrDefault();

            if (customer == null)
                return HttpNotFound();

            return View(customer);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await SupabaseService.InitializeAsync();
                var supabase = SupabaseService.GetClient();

                var getResponse = await supabase
                    .From<Customer>()
                    .Filter("customer_id", Supabase.Postgrest.Constants.Operator.Equals, id)
                    .Get();

                var customer = getResponse.Models.FirstOrDefault();

                if (customer == null)
                    return Json(new { success = false, message = "Không tìm thấy khách hàng." });

                var deleteResponse = await supabase
                    .From<Customer>()
                    .Delete(customer);

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