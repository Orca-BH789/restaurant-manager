
using BASIC_PROJECT_Model.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace BASIC_PROJECT.Areas.Admin.Controllers
{
    public class TableController : Controller
    {
        // GET: Admin/Table

        public async Task<ActionResult> Index()
        {
            await SupabaseService.InitializeAsync();

            var supabase = SupabaseService.GetClient();

            var response = await supabase
                .From<RestaurantTable>()
                .Select("table_id, table_name, creation_date, capacity")
                .Get();

            return View(response?.Models ?? new List<RestaurantTable>());
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Create(RestaurantTable restaurantable)
        {

            // Bây giờ mới kiểm tra IsValid
            if (ModelState.IsValid)
            {
                try
                {
                    await SupabaseService.InitializeAsync();
                    var supabase = SupabaseService.GetClient();
                    restaurantable.Status = "rảnh";
                    var response = await supabase
                        .From<RestaurantTable>()
                        .Insert(restaurantable);

                    if (response.ResponseMessage.IsSuccessStatusCode)
                    {
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Thêm bàn thất bại.");
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, "Lỗi khi tạo bạn : " + ex.Message);
                }
            }

            return View(restaurantable);
        }



        public async Task<ActionResult> Edit(int id)
        {
            await SupabaseService.InitializeAsync();
            var supabase = SupabaseService.GetClient();

            // Lấy khuyến mãi cụ thể theo ID từ Supabase
            var response = await supabase
                .From<RestaurantTable>()
                .Filter("table_id", Supabase.Postgrest.Constants.Operator.Equals, id)
                .Get();

            var ban = response.Models.FirstOrDefault();

            if (ban == null)
            {
                return HttpNotFound();
            }

            return View(ban);
        }

        [HttpPost]
        public async Task<ActionResult> Edit(RestaurantTable ban)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var table = new RestaurantTable
                    {
                        CreationDate = DateTime.Now
                    };
                    await SupabaseService.InitializeAsync();
                    var supabase = SupabaseService.GetClient();

                    // Update the RestaurantTable record in Supabase
                    var response = await supabase
                        .From<RestaurantTable>()
                        .Filter("table_id", Supabase.Postgrest.Constants.Operator.Equals, ban.TableId)
                        .Update(ban);

                    if (response.ResponseMessage.IsSuccessStatusCode)
                    {
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Failed to update RestaurantTable");
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, "An error occurred: " + ex.Message);
                }
            }

            return View(ban);
        }
        public async Task<ActionResult> Delete(int id)
        {
            await SupabaseService.InitializeAsync();
            var supabase = SupabaseService.GetClient();

            // Lấy khuyến mãi cần xóa theo ID
            var response = await supabase
                .From<RestaurantTable>()
                .Filter("table_id", Supabase.Postgrest.Constants.Operator.Equals, id)
                .Get();

            var ban = response.Models.FirstOrDefault();

            if (ban == null)
            {
                return HttpNotFound();
            }

            return View(ban);
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
                    .From<RestaurantTable>()
                    .Filter("table_id", Supabase.Postgrest.Constants.Operator.Equals, id)
                    .Get();

                var ban = getResponse.Models.FirstOrDefault();
                if (ban == null)
                    return Json(new { success = false, message = "Không tìm thấy bàn." });

                // Xóa
                var deleteResponse = await supabase
                    .From<RestaurantTable>()
                    .Delete(ban);

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