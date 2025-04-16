using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ManagementCenter.Models
{
    [Table("courses")]
    public class course
    {
        [Key]
        public int course_id { get; set; }

        [Display(Name = "Giảng Viên")]
        public string tutor { get; set; } 

        [Required(ErrorMessage = "Vui lòng nhập Tên khóa học.")]
        [StringLength(200)]
        [Display(Name = "Tên Khóa Học")]
        public string course_name { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        [Display(Name = "Ngày Bắt Đầu")]
        public DateTime? start_date { get; set; }

        [DataType(DataType.Currency)]
        [Column(TypeName = "decimal(18, 2)")]
        [Range(0, double.MaxValue, ErrorMessage = "Học phí phải là một giá trị dương.")]
        [Display(Name = "Học Phí")]
        public decimal? fee { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Số lượng tối đa phải lớn hơn 0.")]
        [Display(Name = "Số Lượng Tối Đa")]
        public int? max_capacity { get; set; }

        [Display(Name = "Lịch Học")]
        public string schedule { get; set; }



        

        public virtual ICollection<registration> registrations { get; set; }
    }
}