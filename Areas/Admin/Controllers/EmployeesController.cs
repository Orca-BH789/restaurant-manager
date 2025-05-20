using BASIC_PROJECT_Model.Models;
using ClosedXML.Excel;
using Supabase.Postgrest.Exceptions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using static Supabase.Postgrest.Constants;

namespace BASIC_PROJECT.Areas.Admin.Controllers
{
    public class EmployeesController : Controller
    {
        // GET: Admin/Employees
        public async Task<ActionResult> Index()
        {
            await SupabaseService.InitializeAsync();
            var supabase = SupabaseService.GetClient();

            var response = await supabase
                .From<Employee>()
                .Select("employee_id, full_name, email, phone_number, address")
                .Get();

            return View(response?.Models ?? new List<Employee>());
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Create(Employee employee)
        {
            if (string.IsNullOrEmpty(employee.FullName) ||
                string.IsNullOrEmpty(employee.Email) ||
                string.IsNullOrEmpty(employee.PhoneNumber) ||
                string.IsNullOrEmpty(employee.Address))
            {
                ModelState.AddModelError(string.Empty, "Vui lòng điền đầy đủ thông tin nhân viên.");
            }


            if (ModelState.IsValid)
            {
                try
                {
                    await SupabaseService.InitializeAsync();
                    var supabase = SupabaseService.GetClient();

                    var response = await supabase
                        .From<Employee>()
                        .Insert(employee);

                    if (response.ResponseMessage.IsSuccessStatusCode)
                    {
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Thêm nhân viên thất bại.");
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, "Lỗi khi thêm nhân viên: " + ex.Message);
                }
            }

            return View(employee);
        }

        public async Task<ActionResult> Edit(int id)
        {
            await SupabaseService.InitializeAsync();
            var supabase = SupabaseService.GetClient();

            var response = await supabase
                .From<Employee>()
                .Filter("employee_id", Supabase.Postgrest.Constants.Operator.Equals, id)
                .Get();

            var employee = response.Models.FirstOrDefault();

            if (employee == null)
                return HttpNotFound();

            return View(employee);
        }

        [HttpPost]
        public async Task<ActionResult> Edit(Employee employee)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await SupabaseService.InitializeAsync();
                    var supabase = SupabaseService.GetClient();

                    var response = await supabase
                        .From<Employee>()
                        .Filter("employee_id", Supabase.Postgrest.Constants.Operator.Equals, employee.EmployeeId)
                        .Update(employee);

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
                    ModelState.AddModelError(string.Empty, "Lỗi khi cập nhật nhân viên: " + ex.Message);
                }
            }

            return View(employee);
        }

        public async Task<ActionResult> Delete(int id)
        {
            await SupabaseService.InitializeAsync();
            var supabase = SupabaseService.GetClient();

            var response = await supabase
                .From<Employee>()
                .Filter("employee_id", Supabase.Postgrest.Constants.Operator.Equals, id)
                .Get();

            var employee = response.Models.FirstOrDefault();

            if (employee == null)
                return HttpNotFound();

            return View(employee);
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
                    .From<Employee>()
                    .Filter("employee_id", Supabase.Postgrest.Constants.Operator.Equals, id)
                    .Get();

                var employee = getResponse.Models.FirstOrDefault();

                if (employee == null)
                    return Json(new { success = false, message = "Không tìm thấy nhân viên." });

                var deleteResponse = await supabase
                    .From<Employee>()
                    .Delete(employee);

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
        public async Task<ActionResult> ScheduleShift()
        {
            await SupabaseService.InitializeAsync();
            var client = SupabaseService.GetClient();

            var schedRes = await client.From<WorkSchedule>().Get();
            var shiftRes = await client.From<Shift>().Get();
            var empRes = await client.From<Employee>().Get();

            var schedules = schedRes.Models;
            var shifts = shiftRes.Models;
            var employees = empRes.Models;

            ViewBag.Shifts = shifts;
            ViewBag.Employees = employees;

            return View(schedules);
        }

        public async Task<JsonResult> Copytuantruoc(DateTime? date = null)
        {
            try
            {
                await SupabaseService.InitializeAsync();
                var client = SupabaseService.GetClient();

                Dictionary<string, object> parameters = null;

                if (date.HasValue)
                {
                    parameters = new Dictionary<string, object>
            {
                { "p_date", date.Value.ToString("yyyy-MM-dd") }
            };
                }

                var result = await client.Rpc("copy_next_week_schedule", parameters);

                string msg = date.HasValue
                    ? $"Đã sao chép lịch từ ngày {date.Value:dd/MM/yyyy}"
                    : "Dữ liệu tuần sau đã lưu thành công";

                return Json(new { message = msg, data = result }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { message = "Lỗi khi gọi Supabase: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public async Task<JsonResult> SaveOrUpdateEmployee(int? id, string title, string ngay, int resourceId, int check)
        {
            var parsedNgay = DateTime.ParseExact(ngay, "yyyy-MM-dd", CultureInfo.InvariantCulture).AddDays(1);

            try
            {
                await SupabaseService.InitializeAsync();
                var client = SupabaseService.GetClient();

                // 1) Tìm employee theo tên
                var empRes = await client
                    .From<Employee>()
                    .Filter("full_name", Supabase.Postgrest.Constants.Operator.Equals, title)
                    .Get();
                var emp = empRes.Models.FirstOrDefault();
                if (emp == null)
                    return Json(new { success = false, message = "Nhân viên không tồn tại." });

                switch (check)
                {
                    case 1: // Update
                        if (id.HasValue)
                        {
                            var upd = new WorkSchedule
                            {
                                ScheduleId = id.Value,
                                EmployeeId = emp.EmployeeId,
                                Date = parsedNgay,
                                ShiftId = resourceId
                            };
                            await client
                                .From<WorkSchedule>()
                                .Filter("schedule_id", Supabase.Postgrest.Constants.Operator.Equals, id.Value)
                                .Update(upd);
                        }
                        break;

                    case 2: // Insert
                        var ins = new WorkSchedule
                        {
                            EmployeeId = emp.EmployeeId,
                            Date = parsedNgay,
                            ShiftId = resourceId
                        };
                        await client
                            .From<WorkSchedule>()
                            .Insert(ins);
                        break;

                    case 3: // Delete
                        if (id.HasValue)
                        {
                            await client
                                .From<WorkSchedule>()
                                .Filter("schedule_id", Supabase.Postgrest.Constants.Operator.Equals, id.Value)
                                .Delete();
                        }
                        break;

                    default:
                        return Json(new { success = false, message = "Thao tác không hợp lệ." });
                }

                return Json(new { success = true, message = $"Xử lý thành công.{ngay}" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi khi lưu dữ liệu: " + ex.Message });
            }
        }

        public async Task<ActionResult> WorkClock()
        {
            await SupabaseService.InitializeAsync();
            var client = SupabaseService.GetClient();
            var schedRes = await client.From<WorkSchedule>().Get();
            var shiftRes = await client.From<Shift>().Get();
            var empRes = await client.From<Employee>().Get();
            var schedules = schedRes.Models;
            var shifts = shiftRes.Models;
            var employees = empRes.Models;
            ViewBag.Shifts = shifts;
            ViewBag.Employees = employees;

            return View(schedules);
        }


        [HttpPost]
        public async Task<JsonResult> SaveSingleAttendance(List<WorkScheduleDto> updates)
        {
            try
            {
                // Khởi tạo Supabase client
                await SupabaseService.InitializeAsync();
                var client = SupabaseService.GetClient();

                var resultData = new List<object>();

                foreach (var dto in updates)
                {
                    // Nếu Id > 0 thì update, ngược lại insert mới
                    if (dto.Id > 0)
                    {
                        // Lấy bản ghi cũ
                        var oldRec = (await client
                            .From<WorkSchedule>()
                            .Where(x => x.ScheduleId == dto.Id)
                            .Get())
                            .Models
                            .FirstOrDefault();

                        if (oldRec != null)
                        {
                            oldRec.IsPresent = dto.IsPresent;
                            oldRec.ArrivalTime = dto.ArrivalTime;
                            oldRec.DepartureTime = dto.DepartureTime;
                            oldRec.ShiftId = dto.ShiftId;

                            var upd = await client.From<WorkSchedule>().Update(oldRec);
                            var newId = upd.Models.First().ScheduleId;
                            resultData.Add(new { oldId = dto.Id, newId });
                            continue;
                        }

                    }

                    // Insert mới
                    var newRec = new WorkSchedule
                    {
                        EmployeeId = dto.EmployeeId,
                        Date = dto.Date,
                        IsPresent = dto.IsPresent,
                        ArrivalTime = dto.ArrivalTime,
                        DepartureTime = dto.DepartureTime,
                        ShiftId = dto.ShiftId
                    };
                    var ins = await client.From<WorkSchedule>().Insert(newRec);
                    var inserted = ins.Models.First();
                    resultData.Add(new { oldId = dto.Id, newId = inserted.ScheduleId });
                }

                return Json(new { success = true, data = resultData });
            }
            catch (Exception ex)
            {
                // log nếu cần
                return Json(new { success = false, message = ex.Message });
            }
        }


        [HttpPost]
        public async Task<JsonResult> SaveSalary(SalaryTable[] salaries)
        {
            if (salaries == null || salaries.Length == 0)
                return Json(new { success = false, message = "Không có dữ liệu salary" });
            try
            {
                await SupabaseService.InitializeAsync();
                var client = SupabaseService.GetClient();
                var now = DateTime.UtcNow;

                foreach (var item in salaries)
                {
                    Console.WriteLine($"Đang xử lý nhân viên ID: {item.EmployeeId}, Tháng: {item.Month}, Năm: {item.Year}");

                    // Kiểm tra xem đã có bản ghi lương cho nhân viên này trong tháng/năm chưa
                    var response = await client
                        .From<SalaryTable>()
                        .Filter("employeeid", Operator.Equals, item.EmployeeId)
                        .Filter("month", Operator.Equals, item.Month)
                        .Filter("year", Operator.Equals, item.Year)              // Đổi thành PascalCase
                        .Get();

                    Console.WriteLine($"Tìm thấy {response.Models.Count} bản ghi khớp với điều kiện");

                    var existingRes = response.Models.FirstOrDefault();

                    if (existingRes != null)
                    {
                        // Cập nhật lương nếu đã có
                        Console.WriteLine($"Cập nhật lương cho nhân viên ID: {item.EmployeeId}");

                        // Cập nhật thông tin
                        item.Id = existingRes.Id; // Giữ nguyên ID
                        item.CreatedAt = existingRes.CreatedAt; // Giữ nguyên thời gian tạo
                        item.UpdatedAt = now; // Cập nhật thời gian sửa

                        var updateResponse = await client
                            .From<SalaryTable>()
                            .Update(item);

                        Console.WriteLine($"Kết quả cập nhật: {updateResponse != null}");
                    }
                    else
                    {
                        // Tạo mới nếu chưa có
                        Console.WriteLine($"Tạo mới lương cho nhân viên ID: {item.EmployeeId}");

                        // Đặt thời gian tạo và cập nhật
                        item.CreatedAt = now;
                        item.UpdatedAt = now;

                        var insertResponse = await client
                            .From<SalaryTable>()
                            .Insert(item);

                        Console.WriteLine($"Kết quả thêm mới: {insertResponse != null}");
                    }
                }

                return Json(new { success = true, message = "Đã lưu bảng lương thành công!" });
            }
            catch (PostgrestException ex)
            {
                Console.WriteLine("Lỗi Supabase: " + ex.Message);
                return Json(new { success = false, message = "Lỗi Supabase: " + ex.Message });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi khác: " + ex.Message);
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        [HttpGet]
        public async Task<JsonResult> GetSalariesForExport()
        {
            try
            {
                await SupabaseService.InitializeAsync();
                var client = SupabaseService.GetClient();

                // Lấy tất cả bản ghi lương
                var salaries = await client
                    .From<SalaryTable>()
                    .Get();

                // Lấy thông tin nhân viên để kết hợp tên nhân viên
                var employees = await client
                    .From<Employee>()
                    .Get();

                // Kết hợp dữ liệu
                var result = salaries.Models.Select(s => {
                    var employee = employees.Models.FirstOrDefault(e => e.EmployeeId == s.EmployeeId);
                    return new
                    {
                        employeeId = s.EmployeeId,
                        employeeName = employee?.FullName ?? "Không xác định",
                        month = s.Month,
                        year = s.Year,
                        totalHours = s.TotalHours,
                        offDays = s.OffDays,
                        salaryAmount = s.SalaryAmount
                    };
                }).ToList();

                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi lấy dữ liệu lương: " + ex.Message);
                return Json(new { error = true, message = ex.Message });
            }
        }
    }
}
