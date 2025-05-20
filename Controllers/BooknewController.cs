using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Mvc;
using Newtonsoft.Json;
using System.Linq;
using BASIC_PROJECT_Model.Models;
using Supabase.Postgrest;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using Newtonsoft.Json.Serialization;
using Supabase.Interfaces;

namespace BASIC_PROJECT.Controllers
{
    public class BooknewController : Controller
    {
        // GET: Booknew
        public ActionResult Index()
        {
            return View();
        }

        // GET: Booknew/TaiCho

        public async Task<ActionResult> TaiCho()
        {
            try
            {
                await SupabaseService.InitializeAsync();
                var supabase = SupabaseService.GetClient();

                // Fetch all restaurant tables except table_id = 99
                var tableResponse = await supabase
                    .From<RestaurantTable>()
                    .Select("table_id, table_name, status, capacity")
                    .Where(t => t.TableId != 99)
                    .Get();

                var tables = tableResponse?.Models ?? new List<RestaurantTable>();

                // Log the fetched tables to verify table 99 is excluded
                foreach (var table in tables)
                {
                    Console.WriteLine($"Fetched Table: ID={table.TableId}, Name={table.TableName}");
                }

                // Fetch all booking details to get table_id and booking_id
                var bookingDetailResponse = await supabase
                    .From<BookingDetail>()
                    .Select("table_id, booking_id")
                    .Get();

                var bookingDetails = bookingDetailResponse?.Models ?? new List<BookingDetail>();

                // Fetch all bookings to get booking_id and customer_id
                var bookingResponse = await supabase
                    .From<Booking>()
                    .Select("booking_id, customer_id")
                    .Get();

                var bookings = bookingResponse?.Models ?? new List<Booking>();

                var tableCustomerMapping = (from bd in bookingDetails
                                            join b in bookings on bd.BookingId equals b.BookingId
                                            group new { bd, b } by bd.TableId into g
                                            let latestBooking = g.OrderByDescending(x => x.b.BookingId).FirstOrDefault()
                                            select new
                                            {
                                                TableId = g.Key,
                                                CustomerId = latestBooking?.b.CustomerId ?? 0
                                            }).ToList();

                foreach (var item in tableCustomerMapping)
                {
                    Console.WriteLine($"Table {item.TableId} => Latest Customer: {item.CustomerId}");
                }

                // Combine table data with booking information using TableViewModel
                var tableList = tables.Select(t => new TableViewModel
                {
                    CustomerId = (tableCustomerMapping.FirstOrDefault(m => m.TableId == t.TableId)?.CustomerId ?? 0),
                    TableId = t.TableId,
                    TableName = t.TableName,
                    Status = t.Status,
                    Capacity = t.Capacity
                }).ToList();

                // Additional safety: Filter out table 99 in case it slips through
                tableList = tableList.Where(t => t.TableId != 99).ToList();

                ViewBag.Tables = tableList;
                ViewBag.IdUser = Session["User"];
                ViewBag.Message = "List of restaurant tables.";
                return View();
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = $"Error fetching table data: {ex.Message}";
                ViewBag.Tables = new List<TableViewModel>();
                return View();
            }
        }

        public class TableViewModel
        {
            public int CustomerId { get; set; }
            public int TableId { get; set; }
            public string TableName { get; set; }
            public string Status { get; set; }
            public int Capacity { get; set; }
        }

        // GET: Booknew/MangVe
        public ActionResult MangVe()
        {
            return RedirectToAction("DatMon", new { tableId = 99 });
        }



        [Table("combo_price_view")]
        public class ComboPriceView : BaseModel
        {
            [PrimaryKey("combo_id", false)]
            public int ComboId { get; set; }

            [Column("combo_name")]
            public string ComboName { get; set; }

            [Column("description")]
            public string Description { get; set; }

            [Column("price")]
            public decimal Price { get; set; }
        }

        public async Task<ActionResult> DatMon(int tableId)
        {
            try
            {
                await SupabaseService.InitializeAsync();
                var supabase = SupabaseService.GetClient();

                // Fetch table name if tableId corresponds to a restaurant table
                string tableName = "Bàn mang về - MANG VỀ"; // Default for take-away
                if (tableId != 99) // Assuming tableId 99 is for take-away orders
                {
                    var tableResponse = await supabase
                        .From<RestaurantTable>()
                        .Select("*")
                        .Filter("table_id", Constants.Operator.Equals, tableId.ToString())
                        .Get();

                    var table = tableResponse?.Models.FirstOrDefault();
                    if (table != null)
                    {
                        tableName = $"{table.TableName} - {tableId:D2}";
                    }
                }
                // Lấy danh sách món ăn
                var dishesResponse = await supabase
                    .From<Dish>()
                    .Select("*")
                    .Get();
                var dishes = dishesResponse?.Models?.ToList() ?? new List<Dish>();

                // Lấy danh sách categories từ món ăn
                var categories = dishes.Select(d => d.Category).Distinct().ToList();
                // Fetch combos directly from the combo_price_view
                var comboResponse = await supabase
                    .From<ComboPriceView>()
                    .Select("*")
                    .Get();
                List<ComboPriceView> combos = comboResponse?.Models?.ToList() ?? new List<ComboPriceView>();

                // Pass data to the view
                ViewBag.TableId = tableId;
                ViewBag.TableName = tableName;
                ViewBag.Dishes = dishes;
                ViewBag.Categories = categories;
                ViewBag.Combos = combos;
                ViewBag.SessionUser = Session["User"];


                return View();
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = $"Error fetching data: {ex.Message}";
                ViewBag.TableId = tableId;
                ViewBag.TableName = "Bàn mang về - MANG VỀ";
                ViewBag.Combos = new List<object>(); // Empty list of objects
                return View();
            }
        }

        [HttpGet]
        public async Task<JsonResult> GetAllPromotions()
        {
            try
            {
                await SupabaseService.InitializeAsync();
                var supabase = SupabaseService.GetClient();

                var response = await supabase
                    .From<Promotion>()
                    .Select("promotion_name, discount_amount, promotion_id")
                    .Get();

                var promotions = response.Models
                    .Select(p => new
                    {
                        name = p.PromotionName,
                        amount = p.DiscountAmount,
                        id = p.PromotionId
                    })
                    .ToList();

                return Json(new { success = true, promotions }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi khi tải mã giảm giá: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public async Task<JsonResult> CreateInvoice(InvoiceRequest request)
        {
            try
            {
                if (!request.BookingId.HasValue || !request.TotalAmount.HasValue)
                {
                    return Json(new { success = false, message = "Thông tin không hợp lệ" });
                }

                await SupabaseService.InitializeAsync();
                var supabase = SupabaseService.GetClient();

                var employeeId = Convert.ToInt32(Session["User"]);

                var invoice = new Invoice
                {
                    BookingId = request.BookingId.Value,
                    EmployeeId = employeeId,
                    IssueDate = DateTime.Now.Date,
                    PromotionId = request.PromotionId ?? 0,
                    TotalAmount = request.TotalAmount.HasValue ? (int?)request.TotalAmount.Value : 0
                };


                var response = await supabase
                    .From<Invoice>()
                    .Insert(invoice);

                return Json(new { success = true, message = "Hóa đơn đã được tạo thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi khi tạo hóa đơn: " + ex.Message });
            }
        }
        public class InvoiceRequest
        {
            public int? BookingId { get; set; }
            public int? PromotionId { get; set; }
            public decimal? TotalAmount { get; set; }
        }

        // Add a new ViewModel class to represent the promotion data  
        public class PromotionViewModel
        {
            public string Name { get; set; }
            public decimal? Amount { get; set; }
            public int Id { get; set; }
        }
        [HttpPost]
        public ActionResult Payment(PaymentData paymentData) // Remove [FromBody] attribute
        {
            try
            {
                if (paymentData == null || paymentData.Dishes == null)
                {
                    return Json(new { success = false, message = "Invalid payment data" });
                }

                var orderItems = paymentData.Dishes.Select(d => new OrderItem
                {
                    DishId = d.DishId,
                    Name = d.DishName,
                    Price = (decimal)d.Price,
                    Quantity = d.Quantity,
                    Note = d.Note
                }).ToList();

                TempData["OrderItems"] = JsonConvert.SerializeObject(orderItems);
                TempData["TotalAmount"] = (decimal)paymentData.TotalPrice;
                TempData["BookingId"] = paymentData.BookingId;
                TempData["TableInfo"] = paymentData.TableId == 99 ? "Bàn mang về - MANG VỔ" : $"Bàn {paymentData.TableId}";

                return Json(new { success = true, bookingId = paymentData.BookingId });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error processing payment: {ex.Message}" });
            }
        }
        // In BooknewController.cs, modify the PreparePayment action
        [HttpPost]
        public ActionResult PreparePayment(int bookingId, List<OrderItem> orderItems, decimal totalAmount, string tableInfo)
        {
            // Use Session instead of TempData
            Session["BookingId"] = bookingId;
            Session["OrderItems"] = JsonConvert.SerializeObject(orderItems);
            System.Diagnostics.Debug.WriteLine("OrderItems: " + Session["OrderItems"]);
            Session["TotalAmount"] = totalAmount;
            Session["TableInfo"] = tableInfo;

            return Json(new { success = true });
        }

        // In BooknewController.cs, modify the Payment action (GET)
        [HttpGet]
        public ActionResult Payment(int? bookingId)
        {
            var orderItemsJson = Session["OrderItems"]?.ToString();
            var orderItems = string.IsNullOrEmpty(orderItemsJson)
                ? new List<OrderItem>()
                : JsonConvert.DeserializeObject<List<OrderItem>>(orderItemsJson);

            // Pass the List<OrderItem> directly to ViewBag.OrderItems
            ViewBag.OrderItems = orderItems;

            ViewBag.TotalAmount = Session["TotalAmount"] != null ? (decimal)Session["TotalAmount"] : 0;
            ViewBag.BookingId = Session["BookingId"] != null ? (int)Session["BookingId"] : (bookingId ?? 0);
            ViewBag.TableInfo = Session["TableInfo"]?.ToString() ?? "Bàn không xác định";

            // Clear Session data after retrieving to avoid stale data in future requests
            Session.Remove("OrderItems");
            Session.Remove("TotalAmount");
            Session.Remove("BookingId");
            Session.Remove("TableInfo");

            return View();
        }

        // Define OrderItem class
        public class OrderItem
        {
            public int DishId { get; set; }
            public string Name { get; set; }
            public decimal Price { get; set; }
            public int Quantity { get; set; }
            public string Note { get; set; }
        }

        // Define PaymentData and PaymentDishData classes
        public class PaymentData
        {
            public int TableId { get; set; }
            public List<PaymentDishData> Dishes { get; set; }
            public double TotalPrice { get; set; }
            public int BookingId { get; set; }
        }

        public class PaymentDishData
        {
            public int DishId { get; set; }
            public string DishName { get; set; }
            public double Price { get; set; }
            public int Quantity { get; set; }
            public string Note { get; set; }
        }
        [HttpPost]
        public async Task<ActionResult> ConfirmPayment(string orderData, string tableInfo, int bookingId)
        {
            try
            {
                await SupabaseService.InitializeAsync();
                var supabase = SupabaseService.GetClient();

                // Deserialize orderData
                var orderItems = JsonConvert.DeserializeObject<List<OrderItem>>(orderData);
                decimal totalAmount = orderItems.Sum(item => item.Quantity * item.Price);

                // Update booking status to "đã thanh toán"
                var bookingUpdate = new Booking
                {
                    BookingId = bookingId,
                    Status = "chưa thanh toán",
                    TotalAmount = totalAmount
                };

                await supabase
                    .From<Booking>()
                    .Where(x => x.BookingId == bookingId)
                    .Update(bookingUpdate);

                // Update table status to "trống" (empty)
                int tableId = tableInfo.Contains("mang về") ? 99 : int.Parse(tableInfo.Replace("Bàn ", ""));
                if (tableId != 99)
                {
                    var tableUpdate = new RestaurantTable
                    {
                        TableId = tableId,
                        Status = "rảnh"
                    };
                    await supabase
                        .From<RestaurantTable>()
                        .Where(x => x.TableId == tableId)
                        .Update(tableUpdate);
                }

                // Create an invoice
                var invoice = new Invoice
                {
                    BookingId = bookingId,
                    EmployeeId = int.Parse(Session["User"]?.ToString() ?? "1"), // Assuming employee ID from session or default
                    IssueDate = DateTime.Now,
                    PromotionId = 1, // Default or fetch from data
                    TotalAmount = (int)totalAmount,
                };

                await supabase
                    .From<Invoice>()
                    .Insert(invoice);

                // Redirect to a success page or back to table selection
                return RedirectToAction("TaiCho");
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = $"Error confirming payment: {ex.Message}";
                return View("Payment");
            }
        }

        [HttpGet]
        public async Task<ActionResult> GetOrderedDishes(int tableId)
        {
            try
            {
                await SupabaseService.InitializeAsync();
                var supabase = SupabaseService.GetClient();

                // Get customer ID from session
                var idcustomer = Session["User"];
                if (idcustomer == null)
                {
                    return Json(new { success = false, message = "Phiên đăng nhập không hợp lệ" }, JsonRequestBehavior.AllowGet);
                }
                int customerId = int.Parse(idcustomer.ToString());

                // Lấy tất cả các booking chưa thanh toán của user
                var bookingResponse = await supabase
                    .From<BASIC_PROJECT_Model.Models.Booking>()
                    .Select("*")
                    .Where(x => x.CustomerId == customerId && x.Status == "chưa thanh toán")
                    .Get();

                var bookings = bookingResponse?.Models ?? new List<BASIC_PROJECT_Model.Models.Booking>();
                if (!bookings.Any())
                {
                    return Json(new { success = true, dishes = new List<object>() }, JsonRequestBehavior.AllowGet);
                }

                // Lấy chi tiết booking theo table và danh sách booking_id
                var bookingIds = bookings.Select(b => b.BookingId).ToList();

                var bookingDetailsResponse = await supabase
                    .From<BASIC_PROJECT_Model.Models.BookingDetail>()
                    .Select("*")
                    .Get();

                var bookingDetails = bookingDetailsResponse?.Models?
                    .Where(x => x.TableId == tableId && bookingIds.Contains(x.BookingId))
                    .ToList() ?? new List<BASIC_PROJECT_Model.Models.BookingDetail>();

                if (!bookingDetails.Any())
                {
                    return Json(new { success = true, dishes = new List<object>() }, JsonRequestBehavior.AllowGet);
                }

                var detailBookingIds = bookingDetails.Select(x => x.DetailBookingId).ToList();

                // Lấy dish_booking_detail theo các detail_booking_id
                var dishBookingDetailsResponse = await supabase
                    .From<BASIC_PROJECT_Model.Models.DishBookingDetail>()
                    .Select("*")
                    .Get();

                var dishBookingDetails = dishBookingDetailsResponse?.Models?
                    .Where(x => x.Status == true && x.DetailBookingId.HasValue && detailBookingIds.Contains(x.DetailBookingId.Value))
                    .ToList() ?? new List<BASIC_PROJECT_Model.Models.DishBookingDetail>();

                // Lấy thông tin món ăn
                var dishesResponse = await supabase
                    .From<BASIC_PROJECT_Model.Models.Dish>()
                    .Select("*")
                    .Get();

                var dishes = dishesResponse?.Models ?? new List<BASIC_PROJECT_Model.Models.Dish>();

                // JOIN 3 bảng để lấy danh sách món đã gọi
                var orderedDishes = (from bd in bookingDetails
                                     join dbd in dishBookingDetails on bd.DetailBookingId equals dbd.DetailBookingId
                                     join d in dishes on dbd.DishId equals d.DishId
                                     select new
                                     {
                                         dish_id = d.DishId,
                                         dish_name = d.DishName,
                                         price = d.Price,
                                         quantity = dbd.Quantity ?? 1,
                                         booking_id = bd.BookingId,
                                         note = dbd.Note ?? ""
                                     }).ToList();

                return Json(new { success = true, dishes = orderedDishes }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }


        [HttpPost]
        public async Task<ActionResult> ProcessBooking()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("ProcessBooking action called");

                // Đọc dữ liệu JSON từ request
                using (var reader = new System.IO.StreamReader(Request.InputStream))
                {
                    string json = await reader.ReadToEndAsync();
                    // Log toàn bộ JSON thô nhận được từ client
                    System.Diagnostics.Debug.WriteLine("Dữ liệu JSON nhận được từ client:");
                    System.Diagnostics.Debug.WriteLine(json);

                    // Deserialize JSON thành đối tượng BookingData
                    var bookingData = JsonConvert.DeserializeObject<BookingData>(json);
                    if (bookingData == null)
                    {
                        System.Diagnostics.Debug.WriteLine("Lỗi: Không thể deserialize dữ liệu JSON từ client.");
                        return Json(new { success = false, message = "Dữ liệu đặt bàn không hợp lệ" });
                    }

                    // Log dữ liệu đã deserialize
                    System.Diagnostics.Debug.WriteLine("Dữ liệu BookingData sau khi deserialize:");
                    System.Diagnostics.Debug.WriteLine($"CustomerId: {bookingData.CustomerId}");
                    System.Diagnostics.Debug.WriteLine($"BookingDate: {bookingData.BookingDate}");
                    System.Diagnostics.Debug.WriteLine($"ArrivalDate: {bookingData.ArrivalDate}");
                    System.Diagnostics.Debug.WriteLine($"EndTime: {bookingData.EndTime}");
                    System.Diagnostics.Debug.WriteLine($"Type: {bookingData.Type}");
                    System.Diagnostics.Debug.WriteLine($"TotalAmount: {bookingData.TotalAmount}");
                    System.Diagnostics.Debug.WriteLine($"Status: {bookingData.Status}");
                    System.Diagnostics.Debug.WriteLine($"TableId: {bookingData.TableId}");
                    System.Diagnostics.Debug.WriteLine($"TotalPrice: {bookingData.TotalPrice}");
                    System.Diagnostics.Debug.WriteLine("Danh sách món ăn (Dishes):");
                    foreach (var dish in bookingData.Dishes)
                    {
                        System.Diagnostics.Debug.WriteLine($"  - DishId: {dish.DishId}, Quantity: {dish.Quantity}, Note: {dish.Note}, Status: {dish.Status}");
                    }

                    // Kiểm tra Session["User"]
                    var sessionId = Session["User"];
                    if (sessionId == null)
                    {
                        System.Diagnostics.Debug.WriteLine("Lỗi: Session['User'] không tồn tại.");
                        return Json(new { success = false, message = "Phiên đăng nhập không hợp lệ" });
                    }
                    int customerId = int.Parse(sessionId.ToString());
                    System.Diagnostics.Debug.WriteLine($"Session['User'] hợp lệ, CustomerId: {customerId}");

                    // Đảm bảo Note không null
                    foreach (var dish in bookingData.Dishes)
                    {
                        dish.Note = dish.Note ?? "";
                    }

                    // Khởi tạo Supabase client
                    await SupabaseService.InitializeAsync();
                    var supabase = SupabaseService.GetClient();
                    System.Diagnostics.Debug.WriteLine("Supabase client đã được khởi tạo thành công");

                    // Chuẩn bị tham số cho RPC
                    var parameters = new Dictionary<string, object>
            {
                { "p_customer_id", customerId.ToString() },
                { "p_booking_date", DateTime.Parse(bookingData.BookingDate).ToString("yyyy-MM-dd") },
                { "p_arrival_date", DateTime.Parse(bookingData.ArrivalDate).ToString("yyyy-MM-dd") },
                { "p_end_time", DateTime.Parse(bookingData.EndTime).ToString("yyyy-MM-dd HH:mm:ss") },
                { "p_type", bookingData.Type },
                { "p_total_amount", bookingData.TotalAmount },
                { "p_status", bookingData.Status },
                { "p_table_id", bookingData.TableId.ToString() },
                { "p_table_price", bookingData.TotalPrice },
                { "p_dishes", bookingData.Dishes }
            };

                    // Log tham số trước khi gửi đến Supabase
                    System.Diagnostics.Debug.WriteLine("Tham số gửi đến insert_booking_with_dishes:");
                    foreach (var param in parameters)
                    {
                        System.Diagnostics.Debug.WriteLine($"  {param.Key}: {param.Value}");
                    }

                    // Gọi RPC
                    var response = await supabase.Rpc("insert_booking_with_dishes", parameters);
                    System.Diagnostics.Debug.WriteLine("Đặt bàn thành công qua Supabase RPC");
                    // Lấy client Supabase
                    await SupabaseService.InitializeAsync();


                    // Lấy table hiện tại
                    var result = await supabase
                        .From<RestaurantTable>()
                        .Filter("table_id", Supabase.Postgrest.Constants.Operator.Equals, bookingData.TableId)
                        .Get();

                    var table = result.Models.FirstOrDefault();

                    if (table != null)
                    {
                        // Chỉ cập nhật trạng thái bàn
                        table.Status = "đã đặt";

                        var updateResponse = await supabase
                            .From<RestaurantTable>()
                            .Filter("table_id", Supabase.Postgrest.Constants.Operator.Equals, table.TableId)
                            .Update(table);

                        System.Diagnostics.Debug.WriteLine("Cập nhật trạng thái bàn thành công.");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("Không tìm thấy bàn.");
                    }



                    return Json(new { success = true });
                }
            }
            catch (Exception ex)
            {
                // Log lỗi chi tiết
                System.Diagnostics.Debug.WriteLine("Lỗi xảy ra trong ProcessBooking:");
                System.Diagnostics.Debug.WriteLine($"Message: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                return Json(new { success = false, message = $"Lỗi hệ thống: {ex.Message}" });
            }
        }
        [HttpPost]
        public async Task<ActionResult> ProcessBookingOnline()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("ProcessBookingOnline action called");

                using (var reader = new System.IO.StreamReader(Request.InputStream))
                {
                    string json = await reader.ReadToEndAsync();
                    System.Diagnostics.Debug.WriteLine("Dữ liệu JSON nhận được từ client:");
                    System.Diagnostics.Debug.WriteLine(json);

                    var bookingData = JsonConvert.DeserializeObject<BookingData>(json);
                    if (bookingData == null || !bookingData.Dishes.Any())
                    {
                        System.Diagnostics.Debug.WriteLine("Lỗi: Dữ liệu đặt bàn không hợp lệ hoặc không có món.");
                        return Json(new { success = false, message = "Dữ liệu đặt bàn không hợp lệ hoặc không có món" });
                    }

                    var sessionId = Session["User"];
                    if (sessionId == null)
                    {
                        System.Diagnostics.Debug.WriteLine("Lỗi: Session['User'] không tồn tại.");
                        return Json(new { success = false, message = "Phiên đăng nhập không hợp lệ" });
                    }
                    int customerId = int.Parse(sessionId.ToString());
                    System.Diagnostics.Debug.WriteLine($"Session['User'] hợp lệ, CustomerId: {customerId}");

                    foreach (var dish in bookingData.Dishes)
                    {
                        dish.Note = dish.Note ?? "";
                    }

                    await SupabaseService.InitializeAsync();
                    var supabase = SupabaseService.GetClient();
                    System.Diagnostics.Debug.WriteLine("Supabase client đã được khởi tạo thành công");

                    var parameters = new Dictionary<string, object>
            {
                { "p_customer_id", customerId.ToString() },
                { "p_booking_date", DateTime.Parse(bookingData.BookingDate).ToString("yyyy-MM-dd") },
                { "p_arrival_date", DateTime.Parse(bookingData.ArrivalDate).ToString("yyyy-MM-dd") },
                { "p_end_time", DateTime.Parse(bookingData.EndTime).ToString("yyyy-MM-dd HH:mm:ss") },
                { "p_type", bookingData.Type },
                { "p_total_amount", bookingData.TotalAmount },
                { "p_status", bookingData.Status },
                { "p_table_id", bookingData.TableId.ToString() },
                { "p_table_price", bookingData.TotalPrice },
                { "p_dishes", bookingData.Dishes }
            };

                    System.Diagnostics.Debug.WriteLine("Tham số gửi đến insert_booking_with_dishes:");
                    foreach (var param in parameters)
                    {
                        System.Diagnostics.Debug.WriteLine($"  {param.Key}: {param.Value}");
                    }

                    // Call the RPC to insert the booking and dishes
                    var rpcResponse = await supabase.Rpc("insert_booking_with_dishes", parameters);
                    System.Diagnostics.Debug.WriteLine($"Raw RPC response: {rpcResponse.Content}");

                    // Check if the RPC response is null or empty
                    if (string.IsNullOrEmpty(rpcResponse.Content))
                    {
                        System.Diagnostics.Debug.WriteLine("RPC response is null or empty");
                        return Json(new { success = false, message = "Không nhận được phản hồi từ RPC" });
                    }

                    // Parse the RPC response
                    var jsonResponse = JsonConvert.DeserializeObject<List<dynamic>>(rpcResponse.Content);
                    if (jsonResponse == null || jsonResponse.Count == 0 || jsonResponse[0]["booking_id"] == null)
                    {
                        System.Diagnostics.Debug.WriteLine("RPC response không chứa dữ liệu hợp lệ hoặc không có booking_id");
                        return Json(new { success = false, message = "Không nhận được booking_id từ RPC" });
                    }

                    int idbooking = jsonResponse[0]["booking_id"];
                    System.Diagnostics.Debug.WriteLine($"Booking ID từ RPC: {idbooking}");

                    // Fetch the inserted dishes from the database to confirm
                    var bookingDetailsResponse = await supabase
                        .From<BookingDetail>()
                        .Select("detail_booking_id")
                        .Where(x => x.BookingId == idbooking)
                        .Get();

                    var bookingDetail = bookingDetailsResponse?.Models.FirstOrDefault();
                    if (bookingDetail == null)
                    {
                        System.Diagnostics.Debug.WriteLine("Không tìm thấy chi tiết đặt bàn cho booking_id này.");
                        return Json(new { success = false, message = "Không tìm thấy chi tiết đặt bàn cho booking_id này." });
                    }

                    var dishBookingDetailsResponse = await supabase
                        .From<DishBookingDetail>()
                        .Select("dish_id, quantity, note, status")
                        .Where(x => x.DetailBookingId == bookingDetail.DetailBookingId)
                        .Get();

                    // Use a conditional check instead of ?? operator
                    List<object> insertedDishes;
                    if (dishBookingDetailsResponse?.Models != null)
                    {
                        insertedDishes = dishBookingDetailsResponse.Models.Select(d => (object)new
                        {
                            dish_id = d.DishId,
                            quantity = d.Quantity,
                            note = d.Note,
                            status = d.Status
                        }).ToList();
                    }
                    else
                    {
                        insertedDishes = new List<object>();
                    }

                    System.Diagnostics.Debug.WriteLine($"Inserted Dishes: {JsonConvert.SerializeObject(insertedDishes)}");

                    // Store data for the Payment page
                    Session["CurrentBookingId"] = idbooking;
                    ViewBag.idbooking = idbooking;

                    var dishesResponse = await supabase
                        .From<Dish>()
                        .Select("dish_id, dish_name, price")
                        .Get();

                    var dishes = dishesResponse?.Models ?? new List<Dish>();
                    var orderItems = bookingData.Dishes.Select(d =>
                    {
                        var dish = dishes.FirstOrDefault(x => x.DishId == d.DishId);
                        return new OrderItem
                        {
                            DishId = d.DishId,
                            Name = dish?.DishName ?? "Unknown Dish",
                            Price = dish?.Price ?? 0,
                            Quantity = d.Quantity,
                            Note = d.Note
                        };
                    }).ToList();

                    // Use Session instead of TempData (as per previous fix)
                    Session["OrderItems"] = JsonConvert.SerializeObject(orderItems);
                    Session["TotalAmount"] = (decimal)bookingData.TotalAmount;
                    Session["BookingId"] = idbooking;
                    Session["TableInfo"] = bookingData.TableId == 99 ? "Bàn mang về - MANG VỔ" : $"Bàn {bookingData.TableId}";

                    // Return the booking ID and inserted dishes in the JSON response
                    return Json(new { success = true, idbooking = idbooking, insertedDishes = insertedDishes });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Lỗi xảy ra trong ProcessBookingOnline:");
                System.Diagnostics.Debug.WriteLine($"Message: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");

                // Check if the error is related to network connectivity
                if (ex.Message.Contains("ERR_INTERNET_DISCONNECTED") || ex.Message.Contains("could not establish connection"))
                {
                    return Json(new { success = false, message = "Lỗi kết nối mạng. Vui lòng kiểm tra kết nối và thử lại." });
                }

                return Json(new { success = false, message = $"Lỗi hệ thống: {ex.Message}" });
            }
        }
        [HttpPost]
        public async Task<ActionResult> UpdateBookingDishes(int bookingId, string dishesJson)
        {
            try
            {
                await SupabaseService.InitializeAsync();
                var supabase = SupabaseService.GetClient();

                // Fetch detail_booking_id from booking_detail using booking_id
                var bookingDetailResponse = await supabase
                    .From<BookingDetail>()
                    .Select("detail_booking_id")
                    .Where(x => x.BookingId == bookingId)
                    .Get();

                var bookingDetail = bookingDetailResponse?.Models.FirstOrDefault();
                if (bookingDetail == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy chi tiết đặt bàn cho booking_id này." });
                }

                int detailBookingId = bookingDetail.DetailBookingId;

                var dishes = JsonConvert.DeserializeObject<List<DishData>>(dishesJson);

                var parameters = new Dictionary<string, object>
                {
                    { "p_detail_booking_id", detailBookingId },
                    { "p_dishes", dishes }
                };

                await supabase.Rpc("update_booking_dishes", parameters);

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Helper class to deserialize booking data
        public class BookingData
        {
            [JsonProperty("customer_id")]
            public int CustomerId { get; set; }
            [JsonProperty("booking_date")]
            public string BookingDate { get; set; }
            [JsonProperty("arrival_date")]
            public string ArrivalDate { get; set; }
            [JsonProperty("end_time")]
            public string EndTime { get; set; }
            [JsonProperty("type")]
            public string Type { get; set; }
            [JsonProperty("total_amount")]
            public double TotalAmount { get; set; }
            [JsonProperty("status")]
            public string Status { get; set; }
            [JsonProperty("table_id")]
            public int TableId { get; set; }
            [JsonProperty("total_price")]
            public double TotalPrice { get; set; }
            [JsonProperty("dishes")]
            public List<DishData> Dishes { get; set; }
        }

        public class DishData
        {
            [JsonProperty("dish_id")]
            public int DishId { get; set; }
            [JsonProperty("note")]
            public string Note { get; set; }
            [JsonProperty("quantity")]
            public int Quantity { get; set; }
            [JsonProperty("status")]
            public bool Status { get; set; }
        }
    }

    public class OrderItem
    {
        public string Name { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }

    // ViewModel for Combo
    public class ComboViewModel
    {
        public int ComboId { get; set; }
        public string ComboName { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public List<ComboItem> Items { get; set; }
    }

    public class ComboItem
    {
        public string DishName { get; set; }
        public int Quantity { get; set; }
        public string Image { get; set; }
    }

    // Model for joined combo details
    public class ComboDetailWithDish
    {
        public int ComboId { get; set; }
        public int DishId { get; set; }
        public string DishName { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string Image { get; set; }
    }

}