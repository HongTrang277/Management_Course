// Models/CourseViewModel.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ManagementCenter.Models
{
    // Chứa TOÀN BỘ dữ liệu cho form Create/Edit Course
    public class CourseViewModel
    {
        public int course_id { get; set; } // Dùng cho Edit

        [Display(Name = "Giảng Viên")]
        public string? tutor { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập Tên khóa học.")]
        [StringLength(200)]
        [Display(Name = "Tên Khóa Học")]
        public string course_name { get; set; } // Bỏ 'required' keyword nếu target framework cũ hơn .NET 7

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Ngày Bắt Đầu")]
        public DateTime? start_date { get; set; }

        [Display(Name = "Học Phí")]
        public decimal? fee { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Số lượng tối đa phải lớn hơn 0.")]
        [Display(Name = "Số Lượng Tối Đa")]
        public int? max_capacity { get; set; }

        // Danh sách các buổi học người dùng nhập
        public List<CourseSessionInputModel> Sessions { get; set; } = new List<CourseSessionInputModel>();
    }
}