// File: ValidationAttributes/EnsureStartDateIsPresentOrFutureAttribute.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace ManagementCenter.ValidationAttributes // <-- KIỂM TRA LẠI NAMESPACE NÀY
{
    public class EnsureStartDateIsPresentOrFutureAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
         
            if (value is DateTime dateValue)
            {
                // Chỉ so sánh phần ngày (Date), bỏ qua giờ giấc
                if (dateValue.Date < DateTime.Now.Date)
                {
                    // Ngày nhập vào là ngày trong quá khứ -> Lỗi
                    return new ValidationResult(ErrorMessage ?? "Ngày bắt đầu phải là ngày hôm nay hoặc một ngày trong tương lai.");
                }
                else
                {
                    // Ngày hợp lệ (hôm nay hoặc tương lai)
                    return ValidationResult.Success;
                }
            }
         
            return ValidationResult.Success;
        }
    }
}