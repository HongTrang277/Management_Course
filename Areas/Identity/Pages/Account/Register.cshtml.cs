// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable
using System.Diagnostics;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using ManagementCenter.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using ManagementCenter.Data;

namespace ManagementCenter.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserStore<ApplicationUser> _userStore;
        private readonly IUserEmailStore<ApplicationUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly ApplicationDbContext _context;

        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            IUserStore<ApplicationUser> userStore,
            SignInManager<ApplicationUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _context = context;
        }

        
        [BindProperty]
        public InputModel Input { get; set; }

       
        public string ReturnUrl { get; set; }

       
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

       
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required(ErrorMessage = "Vui lòng nhập Họ tên.")]
            [Display(Name = "Họ và Tên")]
            public string FullName { get; set; } // <<< THUỘC TÍNH QUAN TRỌNG



            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

           
            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [Required]
            [Phone]
            public string PhoneNumber { get; set; }

        
            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
        }


      

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (ModelState.IsValid)
            {
                var user = CreateUser();

                await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);

                user.full_name = Input.FullName;
                user.PhoneNumber = Input.PhoneNumber; 

                IdentityResult result = null; // Khai báo ngoài try để có thể dùng sau này
                try
                {
                    result = await _userManager.CreateAsync(user, Input.Password); // <<< Gọi CreateAsync bên trong try

                    Debug.WriteLine($"DEBUG: CreateAsync Result - Succeeded: {result?.Succeeded}");
                    if (result != null && !result.Succeeded)
                    {
                        Debug.WriteLine("DEBUG: CreateAsync Errors:");
                        foreach (var error in result.Errors)
                        {
                            Debug.WriteLine($"- Code: {error.Code}, Description: {error.Description}");
                        }
                    }
                }
                catch (DbUpdateException dbEx) 
                {

                    string innerErrorMessage = dbEx.InnerException?.Message ?? dbEx.Message;
                    _logger?.LogError(dbEx, $"DbUpdateException saving user: {innerErrorMessage}");
                    ModelState.AddModelError(string.Empty, "Database error occurred: " + innerErrorMessage);
                    return Page();
                }
                catch (Exception ex) // <<< Bắt các lỗi khác
                {
                    _logger?.LogError(ex, "Unexpected error during registration.");
                    ModelState.AddModelError(string.Empty, "An unexpected error occurred.");
                    return Page(); 
                }

                if (result != null)
                {
                    if (result.Succeeded)
                    {
                        _logger.LogInformation("User created a new account with password.");

                    
                        try
                        {
                            // Đảm bảo vai trò "Student" đã tồn tại (thông qua seeding trong Program.cs)
                            var roleResult = await _userManager.AddToRoleAsync(user, "Student");
                            if (roleResult.Succeeded)
                            {
                                _logger.LogInformation($"User {user.UserName} was assigned the 'Student' role.");
                            }
                            else
                            {
                                _logger.LogError($"Error assigning 'Student' role to user {user.UserName}.");
                                foreach (var error in roleResult.Errors)
                                {
                                    _logger.LogError($"- Role Assignment Error: {error.Code} - {error.Description}");
                                   
                                }
                             
                            }
                        }
                        catch (Exception roleEx)
                        {
                            _logger.LogError(roleEx, $"Exception assigning 'Student' role to user {user.UserName}.");
                        }
                      
                        try
                        {
                            var newStudentProfile = new student
                            {
                                ApplicationUserId = user.Id,
                                email = user.Email,
                                phone=Input.PhoneNumber??"",
                            };
                            _context.Add(newStudentProfile); // Cần inject _context
                            await _context.SaveChangesAsync();
                            _logger.LogInformation($"TODO: Đã tạo thành công hồ sơ cho user {user.Id}.");
                        }
                        catch (Exception studentEx)
                        {
                            _logger.LogError(studentEx, $"Error creating student profile for user {user.Id}.");
                        }


                        var userId = await _userManager.GetUserIdAsync(user);
                        var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                        var callbackUrl = Url.Page(
                            "/Account/ConfirmEmail",
                            pageHandler: null,
                            values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                            protocol: Request.Scheme);


                        if (_userManager.Options.SignIn.RequireConfirmedAccount)
                        {
                            return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
                        }
                        else
                        {
                            await _signInManager.SignInAsync(user, isPersistent: false);
                            return LocalRedirect(returnUrl);
                        }
                    }
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }

            return Page();
        }

        private ApplicationUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<ApplicationUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(ApplicationUser)}'. " +
                    $"Ensure that '{nameof(ApplicationUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }

        private IUserEmailStore<ApplicationUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<ApplicationUser>)_userStore;
        }
    }
}
