using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Mvc;
using Newtonsoft.Json;
using System.Linq;
using System.Net.Mail;
using BASIC_PROJECT_Model.Models;
using Supabase.Postgrest;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using BASIC_PROJECT.Controllers.helper;
using System.Net;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
using static Supabase.Postgrest.Constants;
using Supabase.Gotrue;

namespace BASIC_PROJECT.Controllers
{
    public class UserController : Controller
    {
        
        public ActionResult Index()
        {
            return View();
        }
        
        public ActionResult DangKy()
        {
            return View();
        }
        [HttpPost]
        public async Task<ActionResult> XuLyDangKy(FormCollection f)
        {
            string hashedPassword = SHA256Helper.GetSHA256Hash(f["password"]);
            try
            {
                await SupabaseService.InitializeAsync();
                var supabase = SupabaseService.GetClient();

                var response1 = await supabase.From<Customer>().Insert(new Customer
                {
                    FullName = f["firstName"] + " " + f["lastName"],

                    Email = f["email"],
                    PhoneNumber = f["phone"]
                });
                int maxCustomerId = (await supabase
                                .From<Customer>()
                                .Order(x => x.CustomerId, Ordering.Descending)
                                .Limit(1)
                                .Get()).Models.FirstOrDefault()?.CustomerId ?? 100000;

                var response = await supabase.From<Account>().Insert(new Account
                {
                    Username = "customer" + maxCustomerId,
                    RoleId = 4,
                    EmployeeId = null, // hoặc null nếu chưa có
                    Password = f["password"],
                    CustomerId = maxCustomerId, // vì trong lỗi ban đầu, bạn truyền null
                    HashPassword = hashedPassword,
                    Email = f["email"]
                });

            }
            catch
            {
               
                   TempData["Message"] = "Đã xảy ra lỗi khi đăng ký.";
                   return RedirectToAction("DangKy");
            }



            return RedirectToAction("Index");
        }

        

        
        public ActionResult LayMatKhau()
        {
            return View();
        }
        [HttpPost]
        public async Task<ActionResult> LayLaiMatKhau(string mail)
        {
            try
            {
                await SupabaseService.InitializeAsync();
                var supabase = SupabaseService.GetClient();

                var random = new Random();
                var newPassword = new string(Enumerable.Repeat("abcdefghijklmnopqrstuvwxyz0123456789", 6)
                                        .Select(s => s[random.Next(s.Length)]).ToArray());

                // Lấy người dùng theo email
                var result = await supabase
                    .From<Account>()
                    .Select("*")
                    .Filter("email", Supabase.Postgrest.Constants.Operator.Equals, mail)
                    .Get();

                var account = result.Models.FirstOrDefault();

                if (account != null)
                {
                    // Cập nhật password mới (ở đây là dạng plain text – nên mã hóa nếu dùng thật)
                    account.Password = newPassword;
                    account.HashPassword = SHA256Helper.GetSHA256Hash(newPassword);
                    // Ghi lại vào database Supabase
                    await supabase.From<Account>().Update(account);

                    // Gửi email chứa mật khẩu mới
                    var smtpClient = new SmtpClient("smtp.gmail.com", 587)
                    {
                        Credentials = new NetworkCredential("2124801030032@student.tdmu.edu.vn", "wiqv jkyi nsau pjge"), // Nên dùng app password
                        EnableSsl = true
                    };

                    var message = new MailMessage
                    {
                        From = new MailAddress("2124801030032@student.tdmu.edu.vn"),
                        Subject = "Thông tin mật khẩu mới",
                        Body = $"<span style='font-size: larger;'>Mật khẩu mới của bạn là:</span> <strong>{newPassword}</strong>",
                        IsBodyHtml = true
                    };
                    message.To.Add(new MailAddress(mail));

                    smtpClient.Send(message);

                    return Json(new { success = true });
                }
                else
                {
                    return Json(new { success = false, message = "Email không tồn tại trong hệ thống." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        

        [HttpPost]
        public async Task<ActionResult> DangNhap(FormCollection f)
        {
            string MatKhau = f["MatKhau"];
            string TenDN = f["TenDN"];
            string hashedPassword = SHA256Helper.GetSHA256Hash(MatKhau);
            
            try
            {
                await SupabaseService.InitializeAsync(); 
                var supabase = SupabaseService.GetClient();

                var result = await supabase
                    .From<Account>()
                    .Select("username,role_id, HashPassword")
                    .Filter("username", Supabase.Postgrest.Constants.Operator.Equals, TenDN)
                    .Filter("HashPassword", Supabase.Postgrest.Constants.Operator.Equals, hashedPassword)
                    .Get();

                var account = result.Models.FirstOrDefault();

                if (account != null)
                {
                    Session["User"] = account.Username;
                    var role = account.RoleId;
                    
                    if (role == 1 || role == 2)
                    {
                        Session["Role"] = "admin";
                        return RedirectToAction("Index", "DishManage");
                    }
                    if (role == 3)
                    {
                        Session["Role"] = "nv";
                        return RedirectToAction("Index", "DishManage");
                    }
                    if ( role == 4){
                        return RedirectToAction("Index", "Booknew");
                    }
                    else
                    {
                            return RedirectToAction("Index", "Home");
                    }
                     
                    
                }
                else
                {
                    ViewBag.kt = "Sai tên đăng nhập hoặc mật khẩu!";
                    return View("Index");
                }
            }
            catch (Exception ex)
            {
                ViewBag.kt = "Lỗi hệ thống: " +ex.Message;
                return View("Index");
            }
        }

       
    }
}