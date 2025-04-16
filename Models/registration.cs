using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ManagementCenter.Models;

namespace ManagementCenter.Models
{
    [Table("registrations")]
    public class registration
    {
        [Key]
        public int registration_id { get; set; }

        [Display(Name = "Học Viên")]
        public int student_id { get; set; } // Khóa ngoại, trỏ đến Students.StudentId

        [Display(Name = "Khóa Học")]
        public int course_id { get; set; } // Khóa ngoại, trỏ đến Courses.CourseId

        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}", ApplyFormatInEditMode = true)]
        [Display(Name = "Ngày Đăng Ký")]
        public DateTime registration_date { get; set; } = DateTime.Now;

        [StringLength(50)]
        [Display(Name = "Trạng Thái")]
        public string status { get; set; }

        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}", ApplyFormatInEditMode = true)]
        [Display(Name = "Ngày Hoàn Thành")]
        public DateTime? completion_date { get; set; }

        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}", ApplyFormatInEditMode = true)]
        [Display(Name = "Ngày Hủy")]
        public DateTime? cancellation_date { get; set; }

        // --- Navigation Properties ---
        [ForeignKey("student_id")]
        public virtual student student { get; set; } // Trỏ đến Student

        [ForeignKey("course_id")]
        public virtual course course { get; set; } // Trỏ đến Course
    }
}