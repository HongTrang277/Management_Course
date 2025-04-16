using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ManagementCenter.Models
{
    [Table("students")] // Đặt lại tên bảng nếu muốn
    public class student
    {
        [Key]
        public int student_id { get; set; }

        // --- Thông tin đăng nhập ---
        [Required(ErrorMessage = "Tên đăng nhập là bắt buộc")]
        [StringLength(100)]
        [Display(Name = "Tên đăng nhập")]
        public string user_name { get; set; }

        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        [StringLength(255)] // Đủ dài cho mật khẩu đã băm
                            // Quan trọng: Thuộc tính này nên lưu trữ MẬT KHẨU ĐÃ BĂM (HASHED), không phải mật khẩu gốc.
        public string password_hash { get; set; }

        // --- Thông tin Profile ---

        [ForeignKey("user_id")]
        public virtual ApplicationUser user { get; set; }

        [StringLength(20)]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ.")]
        [Display(Name = "Số Điện Thoại")]
        public string phone { get; set; }

        [StringLength(255)]
        [EmailAddress(ErrorMessage = "Địa chỉ email không hợp lệ.")]
        [Display(Name = "Email")]
        public string email { get; set; }

        // --- Navigation Properties ---
        // Một Student có nhiều Registration
        public virtual ICollection<registration> registrations { get; set; }
    }
}