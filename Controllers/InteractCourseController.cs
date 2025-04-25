using Microsoft.AspNetCore.Mvc;
using ManagementCenter.Data;
using ManagementCenter.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims; // Cần để lấy User ID
using System.Threading.Tasks;

namespace ManagementCenter.Controllers
{
  
    public class InteractCourseController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public InteractCourseController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private async Task<student> GetCurrentStudent()
        {
            var applicationUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(applicationUserId))
            {
                return null; // Không tìm thấy user đăng nhập
            }

            return await _context.student
                           .FirstOrDefaultAsync(s => s.ApplicationUserId == applicationUserId);
                
        }


        [Authorize(Roles = "Student")]
        public async Task<IActionResult> MyRegistrations()
        {
            var student = await GetCurrentStudent();
            if (student == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy hồ sơ học viên của bạn.";
                return RedirectToAction("Index", "Home"); // Hoặc trang lỗi/thông báo khác
            }

            // Lấy danh sách các đăng ký của học viên này, kèm theo thông tin khóa học
            var registrations = await _context.registration
                                        .Where(r => r.student_id == student.student_id)
                                        .Include(r => r.course) // Nạp thông tin khóa học liên quan
                                        .OrderByDescending(r => r.registration_date) // Sắp xếp theo ngày đăng ký mới nhất
                                        .ToListAsync();

            // Truyền danh sách đăng ký đến View
            return View(registrations); // Cần tạo View: Views/StudentArea/MyRegistrations.cshtml
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> Register(int courseId) // Nhận courseId từ form
        {
            var student = await GetCurrentStudent();
            if (student == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy hồ sơ học viên để đăng ký.";
                return RedirectToAction(nameof(AvailableCourses)); // Quay lại trang danh sách khóa học
            }

            // Kiểm tra khóa học có tồn tại không
            var course = await _context.course.FindAsync(courseId);
            if (course == null)
            {
                TempData["ErrorMessage"] = "Khóa học không hợp lệ.";
                return RedirectToAction(nameof(AvailableCourses));
            }

            // Kiểm tra đã đăng ký và chưa hủy
            bool alreadyRegistered = await _context.registration
                                             .AnyAsync(r => r.student_id == student.student_id
                                                        && r.course_id == courseId
                                                        && r.status != "Cancelled"); // Chỉ tính các đăng ký chưa hủy

            if (alreadyRegistered)
            {
                TempData["WarningMessage"] = $"Bạn đã đăng ký khóa học '{course.course_name}' này rồi.";
                return RedirectToAction(nameof(AvailableCourses));
            }

            // Kiểm tra số lượng tối đa (nếu khóa học có giới hạn)
            if (course.max_capacity.HasValue)
            {
                var currentRegistrations = await _context.registration
                                                  .CountAsync(r => r.course_id == courseId && r.status != "Cancelled");
                if (currentRegistrations >= course.max_capacity.Value)
                {
                    TempData["ErrorMessage"] = $"Rất tiếc, khóa học '{course.course_name}' đã đủ số lượng học viên.";
                    return RedirectToAction(nameof(AvailableCourses));
                }
            }

            // Tạo bản ghi đăng ký mới
            var newRegistration = new registration
            {
                student_id = student.student_id,
                course_id = courseId,
                registration_date = DateTime.Now,
                status = "Registered" // Hoặc "Pending" nếu cần duyệt
            };

            _context.Add(newRegistration);
            try
            {
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Đăng ký thành công khóa học: {course.course_name}!";
                // Chuyển hướng đến trang các khóa đã đăng ký để xem kết quả
                return RedirectToAction(nameof(MyRegistrations));
            }
            catch (DbUpdateException ex)
            {
                // Ghi log lỗi ở đây nếu cần (dùng ILogger)
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi đăng ký. Vui lòng thử lại.";
                return RedirectToAction(nameof(AvailableCourses));
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> CancelRegistration(int registrationId) // Nhận registrationId từ form
        {
            var student = await GetCurrentStudent();
            if (student == null)
            {
                return Unauthorized("Không tìm thấy thông tin học viên.");
            }

            // Tìm bản ghi đăng ký cần hủy
            var registration = await _context.registration
                                       .Include(r => r.course) // Lấy thông tin khóa học để hiển thị thông báo
                                       .FirstOrDefaultAsync(r => r.registration_id == registrationId);

            // Kiểm tra xem đăng ký có tồn tại và có thuộc về học viên đang đăng nhập không
            if (registration == null || registration.student_id != student.student_id)
            {
                TempData["ErrorMessage"] = "Không tìm thấy đăng ký hợp lệ hoặc bạn không có quyền thực hiện thao tác này.";
                return RedirectToAction(nameof(MyRegistrations));
            }

            // Kiểm tra trạng thái hiện tại (ví dụ: không cho hủy nếu đã hủy rồi)
            if (registration.status == "Cancelled")
            {
                TempData["WarningMessage"] = "Đăng ký này đã được hủy trước đó.";
                return RedirectToAction(nameof(MyRegistrations));
            }

            // Thêm các luật nghiệp vụ khác nếu cần (ví dụ: không cho hủy khi khóa học đã bắt đầu)
            // if(registration.course.start_date <= DateTime.Now) { ... return ... }

            // Cập nhật trạng thái và ngày hủy
            registration.status = "Cancelled";
            registration.cancellation_date = DateTime.Now;

            try
            {
                _context.Update(registration); // Đánh dấu là đã thay đổi
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Đã hủy đăng ký khóa học: {registration.course?.course_name ?? "N/A"}.";
            }
            catch (DbUpdateException ex)
            {
                // Ghi log lỗi
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi hủy đăng ký. Vui lòng thử lại.";
            }

            return RedirectToAction(nameof(MyRegistrations)); // Quay lại trang các khóa đã đăng ký
        }


        [Authorize(Roles = "Student")]
        public async Task<IActionResult> LearningZone(int? id) // id là registrationId
        {
            if (id == null)
            {
                return NotFound("Yêu cầu không hợp lệ.");
            }
            int registrationId = id.Value;

            var student = await GetCurrentStudent();
            if (student == null)
            {
                return Unauthorized("Không tìm thấy thông tin học viên.");
            }

            // Tìm đăng ký, đảm bảo là của học viên này và đang hoạt động (chưa hủy)
            var registration = await _context.registration
                                       .Include(r => r.course) // Cần thông tin khóa học
                                       .FirstOrDefaultAsync(r => r.registration_id == registrationId
                                                            && r.student_id == student.student_id
                                                            && r.status == "Registered"); // Chỉ cho phép truy cập nếu đang đăng ký

            if (registration == null)
            {
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập khu vực học tập cho đăng ký này hoặc đăng ký không hợp lệ.";
                return RedirectToAction(nameof(MyRegistrations));
            }

            // Truyền đối tượng registration (bao gồm course) đến View
            return View(registration); // Cần tạo View: Views/StudentArea/LearningZone.cshtml
        }


        [AllowAnonymous]

        public async Task<IActionResult> AvailableCourses(string searchString)
        {
            var coursesQuery = _context.course.AsQueryable();
            ViewData["CurrentFilter"] = searchString;
            if (!String.IsNullOrEmpty(searchString))
            {
                coursesQuery = coursesQuery.Where(c => c.course_name.ToUpper().Contains(searchString.ToUpper())
                                                   || c.tutor.ToUpper().Contains(searchString.ToUpper()));
            }
            coursesQuery = coursesQuery.OrderBy(c => c.start_date);
            var courses = await coursesQuery.ToListAsync();

            HashSet<int> registeredCourseIds = new HashSet<int>();

            if (User.Identity.IsAuthenticated && User.IsInRole("Student"))
            {
                var currentStudent = await GetCurrentStudent();

                if (currentStudent != null)
                {
                    int studentPrimaryKey = currentStudent.student_id; // <-- Lấy student_id (int) từ student

                    registeredCourseIds = await _context.registration
                            .Where(r => r.student_id == studentPrimaryKey && r.status != "Cancelled") // <-- So sánh int == int
                            .Select(r => r.course_id)
                            .ToHashSetAsync();
                }
            }
            ViewBag.RegisteredCourseIds = registeredCourseIds; // Truyền đi

            var courseIds = courses.Select(c => c.course_id).ToList();
            Dictionary<int, int> registrationCounts = new Dictionary<int, int>();
            if (courseIds.Any())
            {
                registrationCounts = await _context.registration
                       .Where(r => courseIds.Contains(r.course_id) && r.status != "Cancelled")
                       .GroupBy(r => r.course_id)
                       .Select(g => new { CourseId = g.Key, Count = g.Count() })
                       .ToDictionaryAsync(x => x.CourseId, x => x.Count);
            }
            ViewBag.RegistrationCounts = registrationCounts;
          
            return View(courses);
        }


        public async Task<IActionResult> CourseDetails(int? id) // id là course_id
        {
            if (id == null)
            {
                return NotFound("Yêu cầu không hợp lệ.");
            }

            // Tìm khóa học theo ID
            var course = await _context.course
                                  .FirstOrDefaultAsync(m => m.course_id == id);
            if (course == null)
            {
                return NotFound("Không tìm thấy khóa học.");
            }

            // (Tùy chọn) Kiểm tra xem sinh viên đã đăng ký khóa này chưa để hiển thị nút phù hợp
            int currentRegistrations = await _context.registration
                                        .CountAsync(r => r.course_id == id.Value && r.status != "Cancelled");

            int? remainingSlots = course.max_capacity.HasValue
                                    ? course.max_capacity.Value - currentRegistrations
                                    : (int?)null; // null nghĩa là không giới hạn

            bool isFull = course.max_capacity.HasValue && remainingSlots.HasValue && remainingSlots.Value <= 0;

            var student = await GetCurrentStudent();
            bool isRegistered = false;
            if (student != null)
            {
                isRegistered = await _context.registration.AnyAsync(r => r.student_id == student.student_id
                                                                       && r.course_id == id.Value
                                                                       && r.status != "Cancelled");
            }

            // Đưa các giá trị tính toán vào ViewBag để View sử dụng
            // ViewBag.CurrentRegistrations = currentRegistrations; // Có thể thêm nếu muốn hiển thị cả số lượng đã ĐK
            ViewBag.RemainingSlots = remainingSlots;
            ViewBag.IsFull = isFull;
            ViewBag.IsRegistered = isRegistered;
            return View(course);
        }
    }
}
