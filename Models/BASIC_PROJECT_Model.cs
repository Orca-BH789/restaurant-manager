using Supabase.Postgrest.Models;
using Supabase.Postgrest.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BASIC_PROJECT_Model.Models
{
            [Table("account")]
            public class Account : BaseModel
            {
                [PrimaryKey("id")]
                public int Id { get; set; }

                [Column("username")]
                [Required(ErrorMessage = "Tên đăng nhập không được để trống")]
                public string Username { get; set; }

                [Column("role_id")]
                public int RoleId { get; set; }

                [Column("employee_id")]
                public int? EmployeeId { get; set; } // nullable

                [Column("password")]
                public string Password { get; set; }

                [Column("customer_id")]
                public int? CustomerId { get; set; } // nullable

                [Column("HashPassword")]
                public string HashPassword { get; set; }

                [Column("email")]
                public string Email { get; set; }
            }


    [Table("bonus_penalty")]
    public class BonusPenalty : BaseModel
    {
        [PrimaryKey("work_date")]
        public DateTime WorkDate { get; set; }

        [Column("employee_id")]
        public int EmployeeId { get; set; }

        [Column("bonus")]
        public string Bonus { get; set; }

        [Column("penalty")]
        public string Penalty { get; set; }

        [Column("note")]
        public string Note { get; set; }
    }

    [Table("booking")]
    public partial class Booking : BaseModel
    {
        [PrimaryKey("booking_id")]
        public int BookingId { get; set; }

        [Column("customer_id")]
        public int? CustomerId { get; set; }

        [Column("booking_date")]
        public DateTime BookingDate { get; set; }

        [Column("arrival_date")]
        public DateTime ArrivalDate { get; set; }

        [Column("end_time")]
        public DateTime? EndTime { get; set; }

        [Column("type")]
        public string Type { get; set; }

        [Column("total_amount")]
        public decimal? TotalAmount { get; set; }

        [Column("status")]
        public string Status { get; set; }
    }

    [Table("booking_detail")]
    public class BookingDetail : BaseModel
    {
        [PrimaryKey("detail_booking_id")]
        public int DetailBookingId { get; set; }

        [Column("booking_id")]
        public int BookingId { get; set; }

        [Column("table_id")]
        public int TableId { get; set; }

        [Column("price")]
        public decimal? Price { get; set; }
        // Add navigation property for Booking
        [Reference(typeof(Booking))]
        public Booking Booking { get; set; }
    }

    [Table("combo")]
    public class Combo : BaseModel
    {
        [PrimaryKey("combo_id")]
        public int ComboId { get; set; }

        [Column("combo_name")]
        public string ComboName { get; set; }

        [Column("description")]
        public string Description { get; set; }
    }

    [Table("combo_detail")]
    public class ComboDetail : BaseModel
    {
        [PrimaryKey("id")]
        public int Id { get; set; }

        [Column("combo_id")]
        public int ComboId { get; set; }

        [Column("dish_id")]
        public int DishId { get; set; }
        
        [Column("quantity")]
        public int Quantity { get; set; }
        public Dish Dish { get; set; }
    }

    [Table("customer")]
    public class Customer : BaseModel
    {
        [PrimaryKey("customer_id")]
        public int CustomerId { get; set; }

        [Column("full_name")]
        public string FullName { get; set; }

        [Column("email")]
        public string Email { get; set; }

        [Column("phone_number")]
        public string PhoneNumber { get; set; }
        [Column("password")]
        public string Password { get; set; }


    }

    [Table("dish")]
    public class Dish : BaseModel
    {
        [PrimaryKey("dish_id")]
        public int DishId { get; set; }

        [Column("dish_name")]
        public string DishName { get; set; }

        [Column("dish_description")]
        public string DishDescription { get; set; }

        [Column("price")]
        public decimal Price { get; set; }

        [Column("unit")]
        public string Unit { get; set; }

        [Column("category")]
        public string Category { get; set; }

        [Column("image")]
        public string Image { get; set; }
    }

    [Table("dish_booking_detail")]
    public class DishBookingDetail : BaseModel
    {
        [PrimaryKey("id")]
        public int Id { get; set; }

        [Column("dish_id")]
        public int? DishId { get; set; }

        [Column("note")]
        public string Note { get; set; }

        [Column("reason")]
        public string Reason { get; set; }

        [Column("detail_booking_id")]
        public int? DetailBookingId { get; set; }

        [Column("quantity")]
        public int? Quantity { get; set; }

        [Column("status")]
        public bool? Status { get; set; }
    }

    [Table("employee")]
    public class Employee : BaseModel
    {
        [PrimaryKey("employee_id")]
        public int EmployeeId { get; set; }

        [Column("full_name")]
        public string FullName { get; set; }

        [Column("phone_number")]
        public string PhoneNumber { get; set; }

        [Column("address")]
        public string Address { get; set; }

        [Column("start_date")]
        public string StartDate { get; set; }

        [Column("schedule_id")]
        public int? ScheduleId { get; set; }
        [Column("email")]
        public string Email { get; set; }
    }

    [Table("invoice")]
    public class Invoice : BaseModel
    {
        [PrimaryKey("invoice_id")]
        public int InvoiceId { get; set; }

        [Column("booking_id")]
        public int BookingId { get; set; }

        [Column("employee_id")]
        public int? EmployeeId { get; set; }

        [Column("issue_date")]
        public DateTime IssueDate { get; set; }

        [Column("promotion_id")]
        public int PromotionId { get; set; }

        [Column("total_amount")]
        public int? TotalAmount { get; set; }

       
    }

    [Table("promotion")]
    public class Promotion : BaseModel
    {
        [PrimaryKey("promotion_id")]
        public int PromotionId { get; set; }

        [Column("promotion_name")]
        public string PromotionName { get; set; }

        [Column("promotion_details")]
        public string PromotionDetails { get; set; }

        [Column("start_date")]
        public DateTime StartDate { get; set; }

        [Column("end_date")]
        public DateTime EndDate { get; set; }

        [Column("discount_amount")]
        public decimal? DiscountAmount { get; set; }

        [Column("discount_percentage")]
        public double? DiscountPercentage { get; set; }

        [Column("min_purchase_quantity")]
        public int? MinPurchaseQuantity { get; set; }
    }

    [Table("promotion_detail")]
    public class PromotionDetail : BaseModel
    {
        [PrimaryKey("promotion_id")]
        public int PromotionId { get; set; }

        [PrimaryKey("dish_id")]
        public int DishId { get; set; }

        [Column("promotion_price")]
        public int? PromotionPrice { get; set; }

        [Column("quantity")]
        public int? Quantity { get; set; }
    }

    [Table("restaurant_table")]
    public class RestaurantTable : BaseModel
    {
        [PrimaryKey("table_id")]
        public int TableId { get; set; }

        [Column("table_name")]
        public new string  TableName { get; set; }

        [Column("status")]
        public string Status { get; set; }

        [Column("creation_date")]
        public DateTime CreationDate { get; set; }

        [Column("capacity")]
        public int Capacity { get; set; }
    }

    [Table("review")]
    public class Review : BaseModel
    {
        [PrimaryKey("review_id")]
        public int ReviewId { get; set; }

        [Column("dish_id")]
        public int? DishId { get; set; }

        [Column("customer_id")]
        public int? CustomerId { get; set; }

        [Column("restaurant_id")]
        public int? RestaurantId { get; set; }

        [Column("rating")]
        public int Rating { get; set; }

        [Column("review_content")]
        public string ReviewContent { get; set; }
    }

    [Table("role")]
    public class Role : BaseModel
    {
        [PrimaryKey("role_id")]
        public int RoleId { get; set; }

        [Column("role_name")]
        public string RoleName { get; set; }
    }

    [Table("shift")]
    public class Shift : BaseModel
    {
        [PrimaryKey("shift_id")]
        public int ShiftId { get; set; }

        [Column("start_time")]
        public TimeSpan StartTime { get; set; }

        [Column("end_time")]
        public TimeSpan EndTime { get; set; }

        [Column("salary")]
        public int? Salary { get; set; }
    }

    [Table("work_schedule")]
    public class WorkSchedule : BaseModel
    {
        [PrimaryKey("schedule_id")]
        public int ScheduleId { get; set; }

        [Column("employee_id")]
        public int EmployeeId { get; set; }

        [Column("date")]
        public DateTime Date { get; set; }

        [Column("shift_id")]
        public int ShiftId { get; set; }

        [Column("arrival_time")]
        public DateTime? ArrivalTime { get; set; }

        [Column("departure_time")]
        public DateTime? DepartureTime { get; set; }

        [Column("is_present")]
        public bool? IsPresent { get; set; }
    }
    public class ComboViewModel
    {
        public int ComboId { get; set; }
        public string ComboName { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public List<ComboItem> Items { get; set; } = new List<ComboItem>();
    }
    public class WorkScheduleDto
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public DateTime Date { get; set; }
        public bool IsPresent { get; set; }
        public DateTime ArrivalTime { get; set; }
        public DateTime DepartureTime { get; set; }
        public int ShiftId { get; set; }
    }
    [Table("salarytable")]
    public class SalaryTable : BaseModel
    {
        [PrimaryKey("salaryid", false)]
        public int Id { get; set; }

        [Column("employeeid")]
        public int EmployeeId { get; set; }

        [Column("month")]
        public int Month { get; set; }

        [Column("year")]
        public int Year { get; set; }

        [Column("totalhours")]
        public float TotalHours { get; set; }

        [Column("offdays")]
        public int OffDays { get; set; }

        [Column("salaryamount")]
        public float SalaryAmount { get; set; }

        [Column("createdat")]
        public DateTime CreatedAt { get; set; }

        [Column("updatedat")]
        public DateTime UpdatedAt { get; set; }
    }


    public class ComboItem
    {
        public string DishName { get; set; }
        public int Quantity { get; set; }
        public string Image { get; set; }
    }

    public class DatMonViewModel
    {
        public int TableId { get; set; }
        public string TableName { get; set; }
        public List<Dish> Dishes { get; set; }
        public List<string> Categories { get; set; }
        public List<ComboViewModel> Combos { get; set; }
        public string ErrorMessage { get; set; }
    }
    public class ComboPriceView
    {
        public int ComboId { get; set; }
        public string ComboName { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
    }
}