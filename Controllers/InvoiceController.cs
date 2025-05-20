using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Mvc;
using Newtonsoft.Json;
using Supabase.Postgrest;
using System.Linq;
using BASIC_PROJECT_Model.Models;

namespace BASIC_PROJECT.Controllers
{
    public class InvoiceController : Controller
    {
        // GET: Invoice
        public async Task<ActionResult> Index()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Index action called in InvoiceController");

                await SupabaseService.InitializeAsync();
                var supabase = SupabaseService.GetClient();
                System.Diagnostics.Debug.WriteLine("Supabase client initialized successfully");

                // Fetch all invoices
                var invoiceResponse = await supabase
                    .From<Invoice>()
                    .Select("*")
                    .Get();

                var invoices = invoiceResponse?.Models ?? new List<Invoice>();
                System.Diagnostics.Debug.WriteLine($"Fetched {invoices.Count} invoices from Supabase");

                if (invoices.Count == 0)
                {
                    System.Diagnostics.Debug.WriteLine("No invoices found in the Invoice table.");
                }

                // Fetch all promotions
                var promotionResponse = await supabase
                    .From<Promotion>()
                    .Select("*")
                    .Get();

                var promotions = promotionResponse?.Models ?? new List<Promotion>();
                System.Diagnostics.Debug.WriteLine($"Fetched {promotions.Count} promotions from Supabase");

                // Fetch all bookings
                var bookingResponse = await supabase
                    .From<Booking>()
                    .Select("*")
                    .Get();

                var bookings = bookingResponse?.Models ?? new List<Booking>();
                System.Diagnostics.Debug.WriteLine($"Fetched {bookings.Count} bookings from Supabase");

                // Fetch all booking details
                var bookingDetailResponse = await supabase
                    .From<BookingDetail>()
                    .Select("*")
                    .Get();

                var bookingDetails = bookingDetailResponse?.Models ?? new List<BookingDetail>();
                System.Diagnostics.Debug.WriteLine($"Fetched {bookingDetails.Count} booking details from Supabase");

                // Fetch all restaurant tables
                var tableResponse = await supabase
                    .From<RestaurantTable>()
                    .Select("*")
                    .Get();

                var tables = tableResponse?.Models ?? new List<RestaurantTable>();
                System.Diagnostics.Debug.WriteLine($"Fetched {tables.Count} restaurant tables from Supabase");

                // Fetch all customers
                var customerResponse = await supabase
                    .From<Customer>()
                    .Select("*")
                    .Get();

                var customers = customerResponse?.Models ?? new List<Customer>();
                System.Diagnostics.Debug.WriteLine($"Fetched {customers.Count} customers from Supabase");

                // Perform the joins in memory to match the SQL query
                var invoiceList = (from i in invoices
                                   join p in promotions on i.PromotionId equals p.PromotionId
                                   join b in bookings on i.BookingId equals b.BookingId
                                   join bd in bookingDetails on b.BookingId equals bd.BookingId
                                   join t in tables on bd.TableId equals t.TableId
                                   join c in customers on b.CustomerId equals c.CustomerId
                                   select new InvoiceViewModel
                                   {
                                       InvoiceId = i.InvoiceId,
                                       IssueDate = i.IssueDate,
                                       TotalAmount = i.TotalAmount ?? 0,
                                       TableName = t.TableName ?? "N/A",
                                       TableId = bd.TableId,
                                       Status = b.Status ?? "N/A",
                                       BookingDate = b.BookingDate,
                                       BookingId = b.BookingId,
                                       Type = b.Type ?? "N/A",
                                       FullName = c.FullName ?? "N/A",
                                       Email = c.Email ?? "N/A",
                                       PhoneNumber = c.PhoneNumber ?? "N/A"
                                   }).ToList();
                ViewBag.Invoices = invoiceList;
                ViewBag.Message = "List of invoices.";
                return View();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error occurred in Index action:");
                System.Diagnostics.Debug.WriteLine($"Message: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");

                ViewBag.ErrorMessage = $"Error fetching invoices: {ex.Message}";
                ViewBag.Invoices = new List<InvoiceViewModel>();
                return View();
            }
        }

        // POST: Approve Invoice
        [HttpPost]
        public async Task<ActionResult> Approve(int bookingId, int tableId)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"Approve action called for Booking ID: {bookingId}, Table ID: {tableId}");

                await SupabaseService.InitializeAsync();
                var supabase = SupabaseService.GetClient();
                System.Diagnostics.Debug.WriteLine("Supabase client initialized successfully");

                // Update the status in the Booking table to "Đã thanh toán"
                var bookingUpdate = new Dictionary<string, object>
                {
                    { "status", "Đã thanh toán" }
                };

                await supabase
                    .From<Booking>()
                    .Where(x => x.BookingId == bookingId)
                    .Set(x => x.Status, "Đã thanh toán")
                    .Update();

                System.Diagnostics.Debug.WriteLine($"Updated Booking ID {bookingId} status to 'Đã thanh toán'");

                // Update the status in the RestaurantTable to "rảnh"
                var tableUpdate = new Dictionary<string, object>
                {
                    { "status", "rảnh" }
                };

                await supabase
                    .From<RestaurantTable>()
                    .Where(x => x.TableId == tableId)
                    .Set(x => x.Status, "rảnh")
                    .Update();

                System.Diagnostics.Debug.WriteLine($"Updated Table ID {tableId} status to 'rảnh'");

                return Json(new { success = true, message = "Hóa đơn đã được duyệt!" });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error occurred in Approve action:");
                System.Diagnostics.Debug.WriteLine($"Message: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");

                return Json(new { success = false, message = $"Lỗi khi duyệt hóa đơn: {ex.Message}" });
            }
        }

        // ViewModel to map the data for the view
        public class InvoiceViewModel
        {
            public int InvoiceId { get; set; }
            public DateTime IssueDate { get; set; }
            public int TotalAmount { get; set; }
            public string TableName { get; set; }
            public int TableId { get; set; }
            public string Status { get; set; }
            public DateTime BookingDate { get; set; }
            public int BookingId { get; set; }
            public string Type { get; set; }
            public string FullName { get; set; }
            public string Email { get; set; }
            public string PhoneNumber { get; set; }
        }
    }
}