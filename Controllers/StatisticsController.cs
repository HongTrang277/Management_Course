using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ManagementCenter.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using Microsoft.Extensions.Logging; // Thêm ILogger nếu cần log lỗi

namespace ManagementCenter.Controllers
{
    [Authorize(Roles = "Admin")]
    public class StatisticsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<StatisticsController> _logger; // Thêm Logger

        public StatisticsController(ApplicationDbContext context, ILogger<StatisticsController> logger) // Inject Logger
        {
            _context = context;
            _logger = logger;
        }

        public IActionResult Index()
        {
            // Lấy các năm có dữ liệu đăng ký khóa học
            var courseRegYears = _context.registration
                                     .Select(r => r.registration_date.Year)
                                     .Distinct();

            // !!! Lưu ý: Không thể lấy năm đăng ký tài khoản chính xác !!!
            // Nếu muốn hiển thị năm gần đây, có thể làm như sau:
            var currentYear = DateTime.Now.Year;
            var allYears = courseRegYears.ToList(); // Lấy năm từ đăng ký KH
            if (!allYears.Contains(currentYear)) allYears.Add(currentYear); // Thêm năm hiện tại nếu chưa có
                                                                            // Sắp xếp và gửi sang View
            ViewBag.AvailableYears = allYears.OrderByDescending(y => y).ToList();

            return View();
        }

        // --- Đổi tên và thêm tham số dataType ---
        [HttpGet]
        public async Task<IActionResult> GetStatisticsData(int year, int? month, string dataType)
        {
            try
            {
                var vietnameseCulture = new CultureInfo("vi-VN");

                if (dataType == "courseReg") // Nếu yêu cầu thống kê đăng ký KHÓA HỌC
                {
                    var query = _context.registration.Where(r => r.registration_date.Year == year);

                    if (month.HasValue && month.Value >= 1 && month.Value <= 12)
                    {
                        // Thống kê theo NGÀY trong THÁNG
                        query = query.Where(r => r.registration_date.Month == month.Value);
                        var dailyData = await query
                            .GroupBy(r => r.registration_date.Day)
                            .Select(g => new { Day = g.Key, Count = g.Count() })
                            .OrderBy(x => x.Day)
                            .ToListAsync();

                        int daysInMonth = DateTime.DaysInMonth(year, month.Value);
                        var labels = Enumerable.Range(1, daysInMonth).Select(d => $"Ngày {d}").ToList();
                        var data = new int[daysInMonth];
                        foreach (var dayStat in dailyData) { if (dayStat.Day >= 1 && dayStat.Day <= daysInMonth) { data[dayStat.Day - 1] = dayStat.Count; } }
                        return Json(new { success = true, labels, data, chartLabel = $"Đăng ký khóa học ({vietnameseCulture.DateTimeFormat.GetMonthName(month.Value)}/{year})" });
                    }
                    else
                    {
                        // Thống kê theo THÁNG trong NĂM
                        var monthlyData = await query
                            .GroupBy(r => r.registration_date.Month)
                            .Select(g => new { Month = g.Key, Count = g.Count() })
                            .OrderBy(x => x.Month)
                            .ToListAsync();

                        var culture = new CultureInfo("vi-VN");
                        var labels = culture.DateTimeFormat.MonthNames.Take(12).ToList();
                        var data = new int[12];
                        foreach (var monthStat in monthlyData) { if (monthStat.Month >= 1 && monthStat.Month <= 12) { data[monthStat.Month - 1] = monthStat.Count; } }
                        return Json(new { success = true, labels, data, chartLabel = $"Đăng ký khóa học (Năm {year})" });
                    }
                }
                else if (dataType == "accountReg") // Nếu yêu cầu thống kê đăng ký TÀI KHOẢN
                {
                    // !!! KHÔNG THỂ THỐNG KÊ CHÍNH XÁC THEO THỜI GIAN VỚI SCHEMA HIỆN TẠI !!!
                    // Trả về thông báo lỗi hoặc dữ liệu rỗng
                    _logger.LogWarning("Attempted to get time-based account registration stats, which is not supported without specific schema changes.");
                    return Json(new { success = false, message = "Chức năng thống kê đăng ký tài khoản theo thời gian chưa được hỗ trợ." });

                    // Hoặc nếu muốn trả về tổng số tài khoản (không theo thời gian):
                    // var totalUsers = await _context.Users.CountAsync();
                    // return Json(new { success = true, totalCount = totalUsers }); // Cần JS xử lý khác
                }
                else
                {
                    return Json(new { success = false, message = "Loại thống kê không hợp lệ." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting statistics data for year {Year}, month {Month}, type {DataType}", year, month, dataType);
                return Json(new { success = false, message = "Lỗi hệ thống khi lấy dữ liệu thống kê." });
            }
        }

        // --- XÓA ACTION GetTopCoursesStats ---
        // [HttpGet]
        // public async Task<IActionResult> GetTopCoursesStats(int top = 5) { ... }

    }
}