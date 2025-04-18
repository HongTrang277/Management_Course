// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using ManagementCenter.Models; // Đảm bảo using đúng namespace model ApplicationUser
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace ManagementCenter.Areas.Identity.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<LoginModel> _logger;
        // *** 1. Thêm UserManager ***
        private readonly UserManager<ApplicationUser> _userManager;

        public LoginModel(
            SignInManager<ApplicationUser> signInManager,
            ILogger<LoginModel> logger,
            UserManager<ApplicationUser> userManager) // *** Inject UserManager vào constructor ***
        {
            _signInManager = signInManager;
            _logger = logger;
            _userManager = userManager; // *** Gán giá trị ***
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public string ReturnUrl { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [Display(Name = "Remember me?")]
            public bool RememberMe { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            returnUrl ??= Url.Content("~/"); // Mặc định là trang chủ (đã đổi thành /InteractCourse/AvailableCourses)

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            ReturnUrl = returnUrl;
        }

        // *** 2. Chỉnh sửa OnPostAsync ***
        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded) // *** Nếu đăng nhập thành công ***
                {
                    _logger.LogInformation($"User '{Input.Email}' logged in.");

                    // --- BẮT ĐẦU: Logic kiểm tra vai trò và chuyển hướng ---
                    var user = await _userManager.FindByEmailAsync(Input.Email);
                    if (user != null)
                    {
                        // Kiểm tra vai trò "Admin"
                        if (await _userManager.IsInRoleAsync(user, "Admin"))
                        {
                            _logger.LogInformation($"Admin user '{Input.Email}' detected. Redirecting to course management.");
                            // Nếu là Admin -> Chuyển đến trang quản lý khóa học
                            return RedirectToAction("Index", "course"); // Chuyển đến CourseController, Action Index
                        }
                        // (Có thể thêm else if cho các vai trò khác ở đây)
                    }
                    // --- KẾT THÚC: Logic kiểm tra vai trò ---

                    // Nếu không phải Admin hoặc có lỗi -> Chuyển hướng mặc định
                    _logger.LogInformation($"Non-admin user or role check failed for '{Input.Email}'. Redirecting to returnUrl: {returnUrl}");
                    return LocalRedirect(returnUrl); // Chuyển hướng đến returnUrl (trang khám phá hoặc trang trước đó)
                }
                // ... (Xử lý lỗi RequiresTwoFactor, IsLockedOut, Invalid login attempt giữ nguyên) ...
                if (result.RequiresTwoFactor)
                {
                    return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
                }
                if (result.IsLockedOut)
                {
                    _logger.LogWarning($"User account for '{Input.Email}' locked out.");
                    return RedirectToPage("./Lockout");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return Page();
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }
    }
}