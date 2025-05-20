using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Mvc;
using Newtonsoft.Json;
using System.Linq;
using System.Net.Mail;
using BASIC_PROJECT_Model.Models;
using Supabase.Postgrest;
using BASIC_PROJECT.Models;

namespace BASIC_PROJECT.Controllers
{
    public class DishManageController : Controller
    {
        // GET: DishManage
        public async Task<ActionResult> Index()
        {

            try
            {
                await SupabaseService.InitializeAsync();
                var supabase = SupabaseService.GetClient();

                var result = await supabase
                    .From<Dish>()
                    .Select("dish_id,dish_name, dish_description, price, unit, category,image")
                    .Get();

                var dishes = result.Models;
                return View(dishes);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Lỗi khi tải dữ liệu: " + ex.Message;
                return View(new List<Dish>());
            }
        }
        [HttpPost]
        public async Task<ActionResult> ThemMonMoi(FormCollection f)
        {
            try
            {
                await SupabaseService.InitializeAsync();
                var supabase = SupabaseService.GetClient();

                var dish = new Dish
                {
                    DishName = f["dishName"],
                    DishDescription = f["dishDescription"],
                    Price = decimal.Parse(f["price"]),
                    Unit = f["unit"],
                    Category = f["category"],
                    Image = f["image"]
                };

                var response = await supabase
                    .From<Dish>()
                    .Insert(dish);

                if (response.Models != null && response.Models.Any())
                {
                    TempData["Message"] = "Thêm món thành công!";
                }
                else
                {
                    TempData["Message"] = "Không thể thêm món. Vui lòng thử lại.";
                }

                return RedirectToAction("index"); // hoặc về lại trang chính
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi thêm món: " + ex.Message);
                TempData["Message"] = "Đã xảy ra lỗi khi thêm món.";
                return RedirectToAction("index");
            }
        }

        public async Task<ActionResult> Details(int id)
        {
            await SupabaseService.InitializeAsync();
            var supabase = SupabaseService.GetClient();
            var result = await supabase.From<Dish>()
                                       .Where(d => d.DishId == id)
                                       .Get();

            var dish = result.Models.FirstOrDefault();

            if (dish == null)
                return HttpNotFound();

            return View(dish);
        }
        
        [HttpPost]
        public async Task<ActionResult> Edit(Dish dis)
        {
                    await SupabaseService.InitializeAsync();
                    var supabase = SupabaseService.GetClient();

                    // Update the promotion record in Supabase
                    var response = await supabase
                        .From<Dish>()
                        .Filter("dish_id", Supabase.Postgrest.Constants.Operator.Equals, dis.DishId)
                        .Update(dis);

                    if (response.ResponseMessage.IsSuccessStatusCode)
                    {
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Failed to update promotion");
                    }



            return RedirectToAction("Index");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                await SupabaseService.InitializeAsync();
                var supabase = SupabaseService.GetClient();

                var getResponse = await supabase
                    .From<Dish>()
                    .Filter("dish_id", Supabase.Postgrest.Constants.Operator.Equals, id)
                    .Get();

                var monan = getResponse.Models.FirstOrDefault();
                if (monan == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy món ăn.";
                    return RedirectToAction("Index");
                }

                var deleteResponse = await supabase
                    .From<Dish>()
                    .Delete(monan);

                if (deleteResponse.ResponseMessage.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Xóa món thành công!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Xóa thất bại!";
                }

                return RedirectToAction("Index"); 
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi khi xóa: " + ex.Message;
                return RedirectToAction("Index");
            }
        }


    }
}
