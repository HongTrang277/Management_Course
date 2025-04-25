// Models/CourseSession.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ManagementCenter.Models
{
    [Table("CourseSessions")]
    public class CourseSession
    {
        [Key]
        public int CourseSessionId { get; set; }

        [Required]
        [Display(Name = "Khóa học")]
        public int CourseId { get; set; } 

        [Required(ErrorMessage = "Vui lòng chọn ngày trong tuần.")]
        [Display(Name = "Thứ")]
        public DayOfWeek DayOfWeek { get; set; } 

        [Required(ErrorMessage = "Vui lòng nhập giờ bắt đầu.")]
        [DataType(DataType.Time)]
        [Display(Name = "Giờ bắt đầu")]
        public TimeOnly StartTime { get; set; }

        [DataType(DataType.Time)]
        [Display(Name = "Giờ kết thúc")]
        public TimeOnly? EndTime { get; set; } 

        [ForeignKey("CourseId")]
        public virtual course Course { get; set; } = null!;
    }
}