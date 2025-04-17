// Models/CourseSessionInputModel.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace ManagementCenter.Models
{
    // Chỉ chứa dữ liệu nhập cho 1 buổi học từ form
    public class CourseSessionInputModel
    {
        [Required(ErrorMessage = "Vui lòng chọn ngày trong tuần.")]
        public DayOfWeek DayOfWeek { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập giờ bắt đầu.")]
        [DataType(DataType.Time)]
        public TimeOnly StartTime { get; set; }

        [DataType(DataType.Time)]
        public TimeOnly? EndTime { get; set; }
    }
}