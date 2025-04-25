using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ManagementCenter.Models
{
    [Table("students")] 
    public class student
    {
        [Key]
        public int student_id { get; set; }

       
        [Required]
        [Display(Name = "User Account")]
        public required string ApplicationUserId { get; set; } 

        [ForeignKey("ApplicationUserId")]
        public virtual ApplicationUser ApplicationUser { get; set; } = null!;

      

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

       
        public virtual ICollection<registration> registrations { get; set; } = new List<registration>();
    }
}