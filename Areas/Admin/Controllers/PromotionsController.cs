using BASIC_PROJECT_Model.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace BASIC_PROJECT.Areas.Admin.Controllers
{
    public class PromotionsController : Controller
    {
        // GET: Admin/KhuyenMai

        public async Task<ActionResult> Index()
        {
            await SupabaseService.InitializeAsync();

            var supabase = SupabaseService.GetClient();

            var response = await supabase
                .From<Promotion>()
                .Select("promotion_id, promotion_name, promotion_details, start_date, end_date, discount_amount, discount_percentage, min_purchase_quantity ")
                .Get();

            return View(response?.Models ?? new List<Promotion>());
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Create(Promotion promotion)
        {
            // Tự kiểm tra và thêm lỗi
            if (promotion.StartDate > promotion.EndDate)
                ModelState.AddModelError(string.Empty, "Ngày bắt đầu không thể lớn hơn ngày kết thúc.");

            if (promotion.DiscountAmount < 0)
                ModelState.AddModelError(string.Empty, "Giảm giá tiền không thể là số âm.");

            if (promotion.MinPurchaseQuantity <= 1)
                ModelState.AddModelError(string.Empty, "Số lượng mua tối thiểu phải lớn hơn 1.");

            if (string.IsNullOrEmpty(promotion.PromotionName) ||
                string.IsNullOrEmpty(promotion.PromotionDetails) ||
                promotion.MinPurchaseQuantity == null)
                ModelState.AddModelError(string.Empty, "Vui lòng nhập đầy đủ thông tin.");

            // Bây giờ mới kiểm tra IsValid
            if (ModelState.IsValid)
            {
                try
                {
                    await SupabaseService.InitializeAsync();
                    var supabase = SupabaseService.GetClient();

                    var response = await supabase
                        .From<Promotion>()
                        .Insert(promotion);

                    if (response.ResponseMessage.IsSuccessStatusCode)
                    {
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Thêm khuyến mãi thất bại.");
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, "Lỗi khi tạo khuyến mãi: " + ex.Message);
                }
            }

            return View(promotion);
        }



        public async Task<ActionResult> Edit(int id)
        {
            await SupabaseService.InitializeAsync();
            var supabase = SupabaseService.GetClient();

            // Lấy khuyến mãi cụ thể theo ID từ Supabase
            var response = await supabase
                .From<Promotion>()
                .Filter("promotion_id", Supabase.Postgrest.Constants.Operator.Equals, id)
                .Get();

            var khuyenMai = response.Models.FirstOrDefault();

            if (khuyenMai == null)
            {
                return HttpNotFound();
            }

            return View(khuyenMai);
        }

        [HttpPost]
        public async Task<ActionResult> Edit(Promotion khuyenMai)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await SupabaseService.InitializeAsync();
                    var supabase = SupabaseService.GetClient();

                    // Update the promotion record in Supabase
                    var response = await supabase
                        .From<Promotion>()
                        .Filter("promotion_id", Supabase.Postgrest.Constants.Operator.Equals, khuyenMai.PromotionId)
                        .Update(khuyenMai);

                    if (response.ResponseMessage.IsSuccessStatusCode)
                    {
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Failed to update promotion");
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, "An error occurred: " + ex.Message);
                }
            }

            return View(khuyenMai);
        }



        public async Task<ActionResult> Delete(int id)
        {
            await SupabaseService.InitializeAsync();
            var supabase = SupabaseService.GetClient();

            // Lấy khuyến mãi cần xóa theo ID
            var response = await supabase
                .From<Promotion>()
                .Filter("promotion_id", Supabase.Postgrest.Constants.Operator.Equals, id)
                .Get();

            var khuyenMai = response.Models.FirstOrDefault();

            if (khuyenMai == null)
            {
                return HttpNotFound();
            }

            return View(khuyenMai);
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
                    .From<Promotion>()
                    .Filter("promotion_id", Supabase.Postgrest.Constants.Operator.Equals, id)
                    .Get();

                var khuyenMai = getResponse.Models.FirstOrDefault();
                if (khuyenMai == null)
                    return Json(new { success = false, message = "Không tìm thấy khuyến mãi." });

                // Xóa
                var deleteResponse = await supabase
                    .From<Promotion>()
                    .Delete(khuyenMai);

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


        [HttpGet]
        public async Task<ActionResult> GetAvailableDiscounts()
        {
            await SupabaseService.InitializeAsync();
            var supabase = SupabaseService.GetClient();

            var now = DateTime.UtcNow;

            var promotions = await supabase
                .From<Promotion>()
                .Where(p => p.StartDate <= now && p.EndDate >= now)
                .Get();

            var results = promotions.Models.Select(p => new
            {
                p.PromotionId,
                p.PromotionName,
                Discount = p.DiscountAmount != null ? $"{p.DiscountAmount} VND" : $"{p.DiscountPercentage}%"
            });

            return Json(results, JsonRequestBehavior.AllowGet);
        }


    }

}